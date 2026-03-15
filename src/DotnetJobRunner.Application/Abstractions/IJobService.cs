using DotnetJobRunner.Application.DTOs;
using DotnetJobRunner.Application.Common;

namespace DotnetJobRunner.Application.Abstractions;

public interface IJobService
{
    Task<JobResponse> CreateAsync(CreateJobRequest request, CancellationToken cancellationToken = default);
    Task<JobResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<JobResponse>> ListAsync(JobQueryRequest query, CancellationToken cancellationToken = default);
    Task<bool> CancelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> RetryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RecurringJobResponse> CreateRecurringAsync(CreateRecurringJobRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecurringJobResponse>> ListRecurringAsync(CancellationToken cancellationToken = default);
    Task<bool> EnableRecurringAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DisableRecurringAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteRecurringAsync(Guid id, CancellationToken cancellationToken = default);
}
