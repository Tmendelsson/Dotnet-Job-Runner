using System.Collections.Concurrent;
using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.Jobs.Options;
using Microsoft.Extensions.Options;

namespace DotnetJobRunner.Application.Services;

public class InMemoryJobExecutionLimiter(IOptions<JobExecutionOptions> options) : IJobExecutionLimiter
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();
    private readonly ConcurrentDictionary<string, Queue<DateTimeOffset>> _rateWindows = new();
    private readonly ConcurrentDictionary<string, object> _rateLocks = new();

    public async Task<IAsyncDisposable> AcquireAsync(string jobType, CancellationToken cancellationToken)
    {
        var normalizedJobType = Normalize(jobType);
        var jobOptions = ResolveOptions(normalizedJobType);
        var semaphore = _semaphores.GetOrAdd(
            normalizedJobType,
            _ => new SemaphoreSlim(Math.Max(1, jobOptions.MaxConcurrency)));

        await semaphore.WaitAsync(cancellationToken);

        try
        {
            await WaitForRateLimitAsync(normalizedJobType, jobOptions, cancellationToken);
            return new JobExecutionLease(semaphore);
        }
        catch
        {
            semaphore.Release();
            throw;
        }
    }

    private async Task WaitForRateLimitAsync(
        string jobType,
        JobTypeExecutionOptions jobOptions,
        CancellationToken cancellationToken)
    {
        if (jobOptions.RateLimitPerMinute <= 0)
        {
            return;
        }

        var window = TimeSpan.FromSeconds(Math.Max(1, jobOptions.RateLimitWindowInSeconds));
        var queue = _rateWindows.GetOrAdd(jobType, _ => new Queue<DateTimeOffset>());
        var syncRoot = _rateLocks.GetOrAdd(jobType, _ => new object());

        while (true)
        {
            TimeSpan? delay = null;

            lock (syncRoot)
            {
                var now = DateTimeOffset.UtcNow;

                while (queue.Count > 0 && now - queue.Peek() >= window)
                {
                    queue.Dequeue();
                }

                if (queue.Count < jobOptions.RateLimitPerMinute)
                {
                    queue.Enqueue(now);
                    return;
                }

                delay = queue.Peek().Add(window) - now;
            }

            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay.Value, cancellationToken);
            }
        }
    }

    private JobTypeExecutionOptions ResolveOptions(string jobType)
    {
        var configuredOptions = options.Value;
        return configuredOptions.Types.TryGetValue(jobType, out var jobOptions)
            ? jobOptions
            : configuredOptions.Default;
    }

    private static string Normalize(string jobType) => jobType.Trim().ToLowerInvariant();

    private sealed class JobExecutionLease(SemaphoreSlim semaphore) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            semaphore.Release();
            return ValueTask.CompletedTask;
        }
    }
}
