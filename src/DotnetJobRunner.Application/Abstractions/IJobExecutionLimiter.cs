namespace DotnetJobRunner.Application.Abstractions;

public interface IJobExecutionLimiter
{
    Task<IAsyncDisposable> AcquireAsync(string jobType, CancellationToken cancellationToken);
}
