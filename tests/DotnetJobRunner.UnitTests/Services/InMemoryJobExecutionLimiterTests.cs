using DotnetJobRunner.Application.Jobs.Options;
using DotnetJobRunner.Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace DotnetJobRunner.UnitTests.Services;

public class InMemoryJobExecutionLimiterTests
{
    [Fact]
    public async Task Should_Acquire_Lease_When_No_Limits_Are_Configured()
    {
        var limiter = new InMemoryJobExecutionLimiter(Options.Create(new JobExecutionOptions()));

        await using var lease = await limiter.AcquireAsync("send-email", CancellationToken.None);

        lease.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_Respect_Max_Concurrency_Per_Job_Type()
    {
        var limiter = new InMemoryJobExecutionLimiter(Options.Create(new JobExecutionOptions
        {
            Types =
            {
                ["generate-report"] = new JobTypeExecutionOptions
                {
                    MaxConcurrency = 1
                }
            }
        }));

        await using var firstLease = await limiter.AcquireAsync("generate-report", CancellationToken.None);
        var secondLeaseTask = limiter.AcquireAsync("generate-report", CancellationToken.None);

        await Task.Delay(100);
        secondLeaseTask.IsCompleted.Should().BeFalse();

        await firstLease.DisposeAsync();
        await using var secondLease = await secondLeaseTask.WaitAsync(TimeSpan.FromSeconds(1));

        secondLease.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_Respect_Rate_Limit_Per_Job_Type()
    {
        var limiter = new InMemoryJobExecutionLimiter(Options.Create(new JobExecutionOptions
        {
            Types =
            {
                ["send-email"] = new JobTypeExecutionOptions
                {
                    MaxConcurrency = 10,
                    RateLimitPerMinute = 1,
                    RateLimitWindowInSeconds = 1
                }
            }
        }));

        await using var firstLease = await limiter.AcquireAsync("send-email", CancellationToken.None);
        var secondLeaseTask = limiter.AcquireAsync("send-email", CancellationToken.None);

        await Task.Delay(100);
        secondLeaseTask.IsCompleted.Should().BeFalse();

        await using var secondLease = await secondLeaseTask.WaitAsync(TimeSpan.FromSeconds(2));

        secondLease.Should().NotBeNull();
    }
}
