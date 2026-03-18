using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Domain;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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

        var attempt = job.RetryCount + 1;
        var startedAt = DateTime.UtcNow;
        var execution = new JobExecution
        {
            JobId = job.Id,
            Attempt = attempt,
            Status = JobStatus.Processing,
            StartedAt = startedAt,
            Log = $"Started execution for job type '{job.Type}'."
        };

        job.Status = JobStatus.Processing;
        job.StartedAt ??= startedAt;
        await repository.UpdateAsync(job, cancellationToken);

        try
        {
            var executionLog = await ExecuteByTypeAsync(job, cancellationToken);
            var finishedAt = DateTime.UtcNow;

            job.Status = JobStatus.Completed;
            job.ErrorMessage = null;
            job.FinishedAt = finishedAt;

            execution.Status = JobStatus.Completed;
            execution.FinishedAt = finishedAt;
            execution.DurationInMs = (long)(finishedAt - startedAt).TotalMilliseconds;
            execution.Log = executionLog;

            await repository.UpdateAsync(job, cancellationToken);
            await repository.AddExecutionAsync(execution, cancellationToken);

            logger.LogInformation("Job {JobId} completed successfully.", jobId);
        }
        catch (Exception ex)
        {
            var finishedAt = DateTime.UtcNow;

            job.RetryCount = attempt;
            job.Status = attempt >= job.MaxRetries ? JobStatus.Failed : JobStatus.Retrying;
            job.ErrorMessage = ex.Message;
            job.FinishedAt = finishedAt;

            execution.Status = job.Status;
            execution.ErrorMessage = ex.Message;
            execution.FinishedAt = finishedAt;
            execution.DurationInMs = (long)(finishedAt - startedAt).TotalMilliseconds;
            execution.Log = $"Execution failed on attempt {attempt}: {ex.Message}";

            await repository.UpdateAsync(job, cancellationToken);
            await repository.AddExecutionAsync(execution, cancellationToken);

            logger.LogError(ex, "Job {JobId} failed during execution.", jobId);
            throw;
        }
    }

    private static async Task<string> ExecuteByTypeAsync(Job job, CancellationToken cancellationToken)
    {
        var jobType = job.Type.Trim().ToLowerInvariant();

        return jobType switch
        {
            "send-email" => await ExecuteSendEmailAsync(job.Payload, cancellationToken),
            "generate-report" => await ExecuteGenerateReportAsync(job.Payload, cancellationToken),
            "import-csv" => await ExecuteImportCsvAsync(job.Payload, cancellationToken),
            "sync-customers" => await ExecuteSyncCustomersAsync(job.Payload, cancellationToken),
            _ => await ExecuteGenericAsync(jobType, cancellationToken)
        };
    }

    private static async Task<string> ExecuteSendEmailAsync(string payloadJson, CancellationToken cancellationToken)
    {
        await Task.Delay(350, cancellationToken);
        var payload = ReadPayload(payloadJson);
        var to = ReadString(payload, "to") ?? "unknown";
        var subject = ReadString(payload, "subject") ?? "(no-subject)";
        return $"Email simulation sent to '{to}' with subject '{subject}'.";
    }

    private static async Task<string> ExecuteGenerateReportAsync(string payloadJson, CancellationToken cancellationToken)
    {
        await Task.Delay(700, cancellationToken);
        var payload = ReadPayload(payloadJson);
        var reportName = ReadString(payload, "reportName") ?? "default-report";
        return $"Report '{reportName}' generated in background simulation.";
    }

    private static async Task<string> ExecuteImportCsvAsync(string payloadJson, CancellationToken cancellationToken)
    {
        await Task.Delay(900, cancellationToken);
        var payload = ReadPayload(payloadJson);
        var fileName = ReadString(payload, "fileName") ?? "import.csv";
        return $"CSV import simulation processed file '{fileName}'.";
    }

    private static async Task<string> ExecuteSyncCustomersAsync(string payloadJson, CancellationToken cancellationToken)
    {
        await Task.Delay(650, cancellationToken);
        var payload = ReadPayload(payloadJson);
        var source = ReadString(payload, "source") ?? "external-system";
        return $"Customer sync simulation completed from source '{source}'.";
    }

    private static async Task<string> ExecuteGenericAsync(string jobType, CancellationToken cancellationToken)
    {
        await Task.Delay(500, cancellationToken);
        return $"Generic execution completed for job type '{jobType}'.";
    }

    private static JsonElement ReadPayload(string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return default;
        }

        try
        {
            using var document = JsonDocument.Parse(payloadJson);
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }
}
