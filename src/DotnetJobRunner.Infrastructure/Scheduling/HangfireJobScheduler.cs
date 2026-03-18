using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.Services;
using Hangfire;

namespace DotnetJobRunner.Infrastructure.Scheduling;

public class HangfireJobScheduler(IBackgroundJobClient backgroundJobs, IRecurringJobManager recurringJobs) : IJobScheduler
{
    public void Enqueue(Guid jobId)
    {
        backgroundJobs.Enqueue<JobExecutionService>(service => service.Execute(jobId, CancellationToken.None));
    }

    public void Schedule(Guid jobId, DateTime runAt)
    {
        backgroundJobs.Schedule<JobExecutionService>(service => service.Execute(jobId, CancellationToken.None), runAt);
    }

    public void Delete(Guid jobId)
    {
        backgroundJobs.Delete(jobId.ToString());
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
