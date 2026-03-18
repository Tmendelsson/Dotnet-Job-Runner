using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Domain;
using Microsoft.EntityFrameworkCore;

namespace DotnetJobRunner.Infrastructure.Persistence;

public class JobRepository(JobDbContext dbContext) : IJobRepository
{
    public async Task<Job> AddAsync(Job job, CancellationToken cancellationToken = default)
    {
        dbContext.Jobs.Add(job);
        await dbContext.SaveChangesAsync(cancellationToken);
        return job;
    }

    public Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Jobs.FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Job> Items, int TotalCount)> QueryAsync(JobStatus? status, string? type, string? priority, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Jobs.AsQueryable();

        if (status is not null)
        {
            query = query.Where(j => j.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(j => j.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(priority))
        {
            var priorityEnum = JobPriorityExtensions.Parse(priority);
            query = query.Where(j => j.Priority == priorityEnum);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task UpdateAsync(Job job, CancellationToken cancellationToken = default)
    {
        dbContext.Jobs.Update(job);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<JobExecution> AddExecutionAsync(JobExecution execution, CancellationToken cancellationToken = default)
    {
        dbContext.JobExecutions.Add(execution);
        await dbContext.SaveChangesAsync(cancellationToken);
        return execution;
    }

    public async Task<IReadOnlyList<JobExecution>> ListExecutionsByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default)
        => await dbContext.JobExecutions
            .Where(x => x.JobId == jobId)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);

    public async Task<RecurringJobDefinition> AddRecurringAsync(RecurringJobDefinition recurringJob, CancellationToken cancellationToken = default)
    {
        dbContext.RecurringJobs.Add(recurringJob);
        await dbContext.SaveChangesAsync(cancellationToken);
        return recurringJob;
    }

    public Task<RecurringJobDefinition?> GetRecurringByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.RecurringJobs.FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public async Task<IReadOnlyList<RecurringJobDefinition>> ListRecurringAsync(CancellationToken cancellationToken = default)
        => await dbContext.RecurringJobs.OrderBy(j => j.Name).ToListAsync(cancellationToken);

    public async Task UpdateRecurringAsync(RecurringJobDefinition recurringJob, CancellationToken cancellationToken = default)
    {
        dbContext.RecurringJobs.Update(recurringJob);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRecurringAsync(RecurringJobDefinition recurringJob, CancellationToken cancellationToken = default)
    {
        dbContext.RecurringJobs.Remove(recurringJob);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
