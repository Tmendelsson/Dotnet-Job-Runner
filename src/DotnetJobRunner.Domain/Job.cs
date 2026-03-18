namespace DotnetJobRunner.Domain;

public class Job
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = "{}";
    public JobPriority Priority { get; set; } = JobPriority.Normal;
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 3;
    public string? ErrorMessage { get; set; }
    public string CreatedBy { get; set; } = "system";

    public ICollection<JobExecution> Executions { get; set; } = new List<JobExecution>();
}
