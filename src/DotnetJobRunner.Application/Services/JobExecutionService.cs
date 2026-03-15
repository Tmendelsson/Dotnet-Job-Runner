using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Domain;
using Microsoft.Extensions.Logging;

namespace DotnetJobRunner.Application.Services;

public class JobExecutionService(IJobRepository repository, ILogger<JobExecutionService> logger)
{
    public async Task Execute(Guid jobId, CancellationToken cancellationToken)
    {
        var job = await repository.GetByIdAsync(jobId, cancellationToken);
        if (job is null || job.Status == JobStatus.Canceled)
        {
            return;
        }

        job.Status = JobStatus.Processing;
        job.StartedAt = DateTime.UtcNow;
        await repository.UpdateAsync(job, cancellationToken);

        try
        {
            // Simula tipos de jobs reais para V1 de portfólio.
            await Task.Delay(1000, cancellationToken);

            if (job.Type.Contains("fail", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Simulated execution error.");
            }

            job.Status = JobStatus.Completed;
            job.FinishedAt = DateTime.UtcNow;
            await repository.UpdateAsync(job, cancellationToken);

            logger.LogInformation("Job {JobId} completed successfully.", jobId);
        }
        catch (Exception ex)
        {
            job.Status = JobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.FinishedAt = DateTime.UtcNow;
            await repository.UpdateAsync(job, cancellationToken);

            logger.LogError(ex, "Job {JobId} failed during execution.", jobId);
            throw;
        }
    }
}
