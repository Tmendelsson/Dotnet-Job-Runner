namespace DotnetJobRunner.Application.Abstractions;

public interface IJobScheduler
{
    void Enqueue(Guid jobId);
    void Schedule(Guid jobId, DateTime runAt);
    void AddOrUpdateRecurring(Guid recurringJobDefinitionId, string cronExpression);
    void RemoveRecurring(Guid recurringJobDefinitionId);
}
