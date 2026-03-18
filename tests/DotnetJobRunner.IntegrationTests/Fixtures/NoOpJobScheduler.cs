using DotnetJobRunner.Application.Abstractions;

namespace DotnetJobRunner.IntegrationTests.Fixtures;

/// <summary>
/// No-op implementation of IJobScheduler for testing.
/// Used in integration tests to avoid actual job scheduling/execution.
/// </summary>
internal sealed class NoOpJobScheduler : IJobScheduler
{
    public void Enqueue(Guid jobId)
    {
    }

    public void Schedule(Guid jobId, DateTime runAt)
    {
    }

    public void Delete(Guid jobId)
    {
    }

    public void AddOrUpdateRecurring(Guid recurringJobDefinitionId, string cronExpression)
    {
    }

    public void RemoveRecurring(Guid recurringJobDefinitionId)
    {
    }
}
