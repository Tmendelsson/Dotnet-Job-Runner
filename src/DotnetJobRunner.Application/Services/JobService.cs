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
            Priority = request.Priority,
            MaxRetries = request.MaxRetries,
            CreatedBy = request.CreatedBy,
            ScheduledAt = request.RunAt,
            Status = request.RunAt is not null
                ? JobStatus.Scheduled
                : JobStatus.Queued
        };

        await repository.AddAsync(job, cancellationToken);

        if (request.RunAt is not null)
        {
            scheduler.Schedule(job.Id, request.RunAt.Value);
        }
        else
        {
            scheduler.Enqueue(job.Id);
        }

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

    public async Task<bool> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await repository.GetByIdAsync(id, cancellationToken);
        if (job is null)
        {
            return false;
        }

        if (job.Status is JobStatus.Completed or JobStatus.Canceled)
        {
            return true;
        }

        if (job.Status == JobStatus.Scheduled)
        {
            scheduler.RemoveRecurring(job.Id);
        }

        job.Status = JobStatus.Canceled;
        job.FinishedAt = DateTime.UtcNow;
        await repository.UpdateAsync(job, cancellationToken);
        return true;
    }

    public async Task<bool> RetryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await repository.GetByIdAsync(id, cancellationToken);
        if (job is null)
        {
            return false;
        }

        if (job.Status != JobStatus.Failed)
        {
            return true;
        }

        job.Status = JobStatus.Retrying;
        job.ErrorMessage = null;
        job.RetryCount += 1;
        await repository.UpdateAsync(job, cancellationToken);

        scheduler.Enqueue(job.Id);
        return true;
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
            Priority = request.Priority,
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

    private static JobResponse Map(Job job) => new()
    {
        Id = job.Id,
        Type = job.Type,
        Status = job.Status,
        Priority = job.Priority,
        CreatedAt = job.CreatedAt,
        ScheduledAt = job.ScheduledAt,
        StartedAt = job.StartedAt,
        FinishedAt = job.FinishedAt,
        RetryCount = job.RetryCount,
        MaxRetries = job.MaxRetries,
        ErrorMessage = job.ErrorMessage
    };
}
