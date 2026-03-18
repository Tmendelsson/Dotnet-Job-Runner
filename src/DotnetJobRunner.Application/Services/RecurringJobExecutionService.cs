using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Domain;
using Microsoft.Extensions.Logging;

namespace DotnetJobRunner.Application.Services;

public class RecurringJobExecutionService(IJobRepository repository, IJobScheduler scheduler, ILogger<RecurringJobExecutionService> logger)
{
    public async Task Execute(Guid recurringJobDefinitionId, CancellationToken cancellationToken)
    {
        var recurring = await repository.GetRecurringByIdAsync(recurringJobDefinitionId, cancellationToken);
        if (recurring is null || !recurring.IsActive)
        {
            return;
        }

        var job = new Job
        {
            Type = recurring.Type,
            Payload = recurring.Payload,
            Priority = recurring.Priority,
            CreatedBy = recurring.CreatedBy,
            Status = JobStatus.Queued,
            MaxRetries = recurring.MaxRetries
        };

        await repository.AddAsync(job, cancellationToken);
        recurring.LastRunAt = DateTime.UtcNow;
        await repository.UpdateRecurringAsync(recurring, cancellationToken);
        scheduler.Enqueue(job.Id);
        logger.LogInformation("Recurring job definition {RecurringJobDefinitionId} materialized as job {JobId}.", recurringJobDefinitionId, job.Id);
    }
}
