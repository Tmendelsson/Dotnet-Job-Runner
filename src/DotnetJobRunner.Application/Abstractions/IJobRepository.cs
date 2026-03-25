using DotnetJobRunner.Domain;

namespace DotnetJobRunner.Application.Abstractions;

public interface IJobRepository
{
    Task<Job> AddAsync(Job job, CancellationToken cancellationToken = default);
    Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Job> Items, int TotalCount)> QueryAsync(JobStatus? status, string? type, string? priority, int page, int pageSize, CancellationToken cancellationToken = default);
    Task UpdateAsync(Job job, CancellationToken cancellationToken = default);
    Task<JobExecution> AddExecutionAsync(JobExecution execution, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<JobExecution> Items, int TotalCount)> ListExecutionsByJobIdAsync(Guid jobId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<RecurringJobDefinition> AddRecurringAsync(RecurringJobDefinition recurringJob, CancellationToken cancellationToken = default);
    Task<RecurringJobDefinition?> GetRecurringByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecurringJobDefinition>> ListRecurringAsync(CancellationToken cancellationToken = default);
    Task UpdateRecurringAsync(RecurringJobDefinition recurringJob, CancellationToken cancellationToken = default);
    Task DeleteRecurringAsync(RecurringJobDefinition recurringJob, CancellationToken cancellationToken = default);
}
