namespace DotnetJobRunner.Application.Abstractions;

public interface IJobScheduler
{
    string Enqueue(Guid jobId);
    string Schedule(Guid jobId, DateTime runAt);
    void Delete(string? hangfireJobId);
    void AddOrUpdateRecurring(Guid recurringJobDefinitionId, string cronExpression);
    void RemoveRecurring(Guid recurringJobDefinitionId);
}
