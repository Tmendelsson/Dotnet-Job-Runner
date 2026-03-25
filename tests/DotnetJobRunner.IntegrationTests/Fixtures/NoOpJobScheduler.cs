using DotnetJobRunner.Application.Abstractions;

namespace DotnetJobRunner.IntegrationTests.Fixtures;

/// <summary>
/// No-op implementation of IJobScheduler for testing.
/// Used in integration tests to avoid actual job scheduling/execution.
/// </summary>
internal sealed class NoOpJobScheduler : IJobScheduler
{
    public string Enqueue(Guid jobId) => string.Empty;

    public string Schedule(Guid jobId, DateTime runAt) => string.Empty;

    public void Delete(string? hangfireJobId)
    {
    }

    public void AddOrUpdateRecurring(Guid recurringJobDefinitionId, string cronExpression)
    {
    }

    public void RemoveRecurring(Guid recurringJobDefinitionId)
    {
    }
}
