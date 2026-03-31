using System.Text.Json;
using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.Common;
using DotnetJobRunner.Application.DTOs;
using DotnetJobRunner.Domain;
using FluentValidation;

namespace DotnetJobRunner.Application.Services;

public class JobService(
    IJobRepository repository,
    IValidator<CreateJobRequest> validator,
    IValidator<CreateRecurringJobRequest> recurringValidator,
    IJobScheduler scheduler) : IJobService
{
    public async Task<JobResponse> CreateAsync(CreateJobRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var job = new Job
        {
            Type = request.Type,
            Payload = JsonSerializer.Serialize(request.Payload ?? new { }),
            Priority = JobPriorityExtensions.Parse(request.Priority),
            MaxRetries = request.MaxRetries,
            CreatedBy = request.CreatedBy,
            ScheduledAt = request.RunAt,
            Status = request.RunAt is not null
                ? JobStatus.Scheduled
                : JobStatus.Queued
        };

        await repository.AddAsync(job, cancellationToken);

        string hangfireJobId;
        if (request.RunAt is not null)
        {
            hangfireJobId = scheduler.Schedule(job.Id, request.RunAt.Value);
        }
        else
        {
            hangfireJobId = scheduler.Enqueue(job.Id);
        }

        job.HangfireJobId = hangfireJobId;
        await repository.UpdateAsync(job, cancellationToken);

        return Map(job);
    }

    public async Task<JobResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await repository.GetByIdAsync(id, cancellationToken);
        return job is null ? null : Map(job);
    }

    public async Task<PagedResult<JobResponse>> ListAsync(JobQueryRequest query, CancellationToken cancellationToken = default)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 100);
        var result = await repository.QueryAsync(query.Status, query.Type, query.Priority, page, pageSize, cancellationToken);

        return new PagedResult<JobResponse>
        {
            Items = result.Items.Select(Map).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<JobOperationResult> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await repository.GetByIdAsync(id, cancellationToken);
        if (job is null)
        {
            return JobOperationResult.NotFound;
        }

        if (job.Status is JobStatus.Completed or JobStatus.Canceled)
        {
            return JobOperationResult.InvalidState;
        }

        if (job.Status == JobStatus.Scheduled)
        {
            scheduler.Delete(job.HangfireJobId);
        }

        job.Status = JobStatus.Canceled;
        job.FinishedAt = DateTime.UtcNow;
        await repository.UpdateAsync(job, cancellationToken);
        return JobOperationResult.Success;
    }

    public async Task<JobOperationResult> RetryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await repository.GetByIdAsync(id, cancellationToken);
        if (job is null)
        {
            return JobOperationResult.NotFound;
        }

        if (job.Status != JobStatus.Failed)
        {
            return JobOperationResult.InvalidState;
        }

        job.Status = JobStatus.Retrying;
        job.ErrorMessage = null;
        job.RetryCount += 1;

        var delaySeconds = 30 * Math.Pow(2, job.RetryCount - 1);
        var runAt = DateTime.UtcNow.AddSeconds(delaySeconds);
        job.HangfireJobId = scheduler.Schedule(job.Id, runAt);
        job.ScheduledAt = runAt;

        await repository.UpdateAsync(job, cancellationToken);
        return JobOperationResult.Success;
    }

    public async Task<PagedResult<ExecutionResponse>?> GetExecutionsAsync(Guid jobId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var job = await repository.GetByIdAsync(jobId, cancellationToken);
        if (job is null)
        {
            return null;
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

        var (items, totalCount) = await repository.ListExecutionsByJobIdAsync(jobId, normalizedPage, normalizedPageSize, cancellationToken);
        return new PagedResult<ExecutionResponse>
        {
            Items = items.Select(MapExecution).ToList(),
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            TotalCount = totalCount
        };
    }

    public async Task<RecurringJobResponse> CreateRecurringAsync(CreateRecurringJobRequest request, CancellationToken cancellationToken = default)
    {
        await recurringValidator.ValidateAndThrowAsync(request, cancellationToken);

        var recurringJob = new RecurringJobDefinition
        {
            Name = request.Name,
            Type = request.Type,
            CronExpression = request.CronExpression,
            Payload = JsonSerializer.Serialize(request.Payload ?? new { }),
            Priority = JobPriorityExtensions.Parse(request.Priority),
            MaxRetries = request.MaxRetries,
            CreatedBy = request.CreatedBy,
            IsActive = true
        };

        await repository.AddRecurringAsync(recurringJob, cancellationToken);
        scheduler.AddOrUpdateRecurring(recurringJob.Id, recurringJob.CronExpression);

        return RecurringJobMapper.Map(recurringJob);
    }

    public async Task<IReadOnlyList<RecurringJobResponse>> ListRecurringAsync(CancellationToken cancellationToken = default)
    {
        var jobs = await repository.ListRecurringAsync(cancellationToken);
        return jobs.Select(RecurringJobMapper.Map).ToList();
    }

    public async Task<bool> EnableRecurringAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var recurringJob = await repository.GetRecurringByIdAsync(id, cancellationToken);
        if (recurringJob is null)
        {
            return false;
        }

        recurringJob.IsActive = true;
        await repository.UpdateRecurringAsync(recurringJob, cancellationToken);
        scheduler.AddOrUpdateRecurring(recurringJob.Id, recurringJob.CronExpression);
        return true;
    }

    public async Task<bool> DisableRecurringAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var recurringJob = await repository.GetRecurringByIdAsync(id, cancellationToken);
        if (recurringJob is null)
        {
            return false;
        }

        recurringJob.IsActive = false;
        await repository.UpdateRecurringAsync(recurringJob, cancellationToken);
        scheduler.RemoveRecurring(recurringJob.Id);
        return true;
    }

    public async Task<bool> DeleteRecurringAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var recurringJob = await repository.GetRecurringByIdAsync(id, cancellationToken);
        if (recurringJob is null)
        {
            return false;
        }

        scheduler.RemoveRecurring(recurringJob.Id);
        await repository.DeleteRecurringAsync(recurringJob, cancellationToken);
        return true;
    }

    private static ExecutionResponse MapExecution(JobExecution e) => new()
    {
        Id = e.Id,
        JobId = e.JobId,
        Attempt = e.Attempt,
        Status = e.Status,
        StartedAt = e.StartedAt,
        FinishedAt = e.FinishedAt,
        DurationInMs = e.DurationInMs,
        Log = e.Log,
        ErrorMessage = e.ErrorMessage
    };

    private static JobResponse Map(Job job) => new()
    {
        Id = job.Id,
        Type = job.Type,
        Status = job.Status,
        Priority = JobPriorityExtensions.ToString(job.Priority),
        CreatedAt = job.CreatedAt,
        ScheduledAt = job.ScheduledAt,
        StartedAt = job.StartedAt,
        FinishedAt = job.FinishedAt,
        RetryCount = job.RetryCount,
        MaxRetries = job.MaxRetries,
        ErrorMessage = job.ErrorMessage
    };
}
