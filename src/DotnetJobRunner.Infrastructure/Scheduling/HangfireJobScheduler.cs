using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.Services;
using Hangfire;

namespace DotnetJobRunner.Infrastructure.Scheduling;

public class HangfireJobScheduler(IBackgroundJobClient backgroundJobs, IRecurringJobManager recurringJobs) : IJobScheduler
{
    public string Enqueue(Guid jobId)
    {
        return backgroundJobs.Enqueue<JobExecutionService>(service => service.Execute(jobId, CancellationToken.None));
    }

    public string Schedule(Guid jobId, DateTime runAt)
    {
        return backgroundJobs.Schedule<JobExecutionService>(service => service.Execute(jobId, CancellationToken.None), runAt);
    }

    public void Delete(string? hangfireJobId)
    {
        if (string.IsNullOrEmpty(hangfireJobId)) return;
        backgroundJobs.Delete(hangfireJobId);
    }

    public void AddOrUpdateRecurring(Guid recurringJobDefinitionId, string cronExpression)
    {
        recurringJobs.AddOrUpdate<RecurringJobExecutionService>(
            $"recurring:{recurringJobDefinitionId}",
            service => service.Execute(recurringJobDefinitionId, CancellationToken.None),
            cronExpression);
    }

    public void RemoveRecurring(Guid recurringJobDefinitionId)
    {
        recurringJobs.RemoveIfExists($"recurring:{recurringJobDefinitionId}");
    }
}
