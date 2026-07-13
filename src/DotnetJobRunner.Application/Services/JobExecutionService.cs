using System.Text.Json;
using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Domain;
using Microsoft.Extensions.Logging;

namespace DotnetJobRunner.Application.Services;

public class JobExecutionService(
    IJobRepository repository,
    IJobHandlerResolver handlerResolver,
    IJobExecutionLimiter executionLimiter,
    ILogger<JobExecutionService> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task Execute(Guid jobId, CancellationToken cancellationToken)
    {
        var job = await repository.GetByIdAsync(jobId, cancellationToken);
        if (job is null || job.Status == JobStatus.Canceled)
        {
            return;
        }

        var attempt = job.RetryCount + 1;
        var startedAt = DateTime.UtcNow;

        var execution = new JobExecution
        {
            JobId = job.Id,
            Attempt = attempt,
            Status = ExecutionStatus.Processing,
            StartedAt = startedAt,
            Log = $"Started execution for job type '{job.Type}'."
        };

        job.Status = JobStatus.Processing;
        job.StartedAt ??= startedAt;
        await repository.UpdateAsync(job, cancellationToken);

        try
        {
            var handler = handlerResolver.Resolve(job.Type);
            var payload = DeserializePayload(job.Payload, handler.PayloadType, job.Type);

            var context = new JobExecutionContext
            {
                JobId = job.Id,
                JobType = job.Type,
                Attempt = attempt,
                StartedAt = startedAt,
                CreatedBy = job.CreatedBy
            };

            await using var lease = await executionLimiter.AcquireAsync(job.Type, cancellationToken);
            var result = await handler.ExecuteAsync(payload, context, cancellationToken);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(result.ErrorMessage ?? result.Log);
            }

            var finishedAt = DateTime.UtcNow;

            job.Status = JobStatus.Completed;
            job.ErrorMessage = null;
            job.FinishedAt = finishedAt;

            execution.Status = ExecutionStatus.Completed;
            execution.FinishedAt = finishedAt;
            execution.DurationInMs = (long)(finishedAt - startedAt).TotalMilliseconds;
            execution.Log = result.Log;

            await repository.UpdateAsync(job, cancellationToken);
            await repository.AddExecutionAsync(execution, cancellationToken);

            logger.LogInformation(
                "Job {JobId} of type {JobType} completed successfully.",
                jobId,
                job.Type);
        }
        catch (Exception ex)
        {
            var finishedAt = DateTime.UtcNow;

            job.RetryCount = attempt;
            job.Status = JobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.FinishedAt = finishedAt;

            execution.Status = ExecutionStatus.Failed;
            execution.ErrorMessage = ex.Message;
            execution.FinishedAt = finishedAt;
            execution.DurationInMs = (long)(finishedAt - startedAt).TotalMilliseconds;
            execution.Log = $"Execution failed on attempt {attempt}: {ex.Message}";

            await repository.UpdateAsync(job, cancellationToken);
            await repository.AddExecutionAsync(execution, cancellationToken);

            logger.LogError(
                ex,
                "Job {JobId} of type {JobType} failed during execution.",
                jobId,
                job.Type);

            throw;
        }
    }

    private static object DeserializePayload(string payloadJson, Type payloadType, string jobType)
    {
        try
        {
            var payload = JsonSerializer.Deserialize(payloadJson, payloadType, JsonOptions);

            return payload ?? throw new InvalidOperationException(
                $"Payload for job type '{jobType}' could not be deserialized.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Invalid payload JSON for job type '{jobType}'.",
                ex);
        }
    }
}
