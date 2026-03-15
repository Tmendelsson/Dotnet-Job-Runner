namespace DotnetJobRunner.Domain;

public enum JobStatus
{
	Pending = 1,
	Queued = 2,
	Scheduled = 3,
	Processing = 4,
	Completed = 5,
	Failed = 6,
	Canceled = 7,
	Retrying = 8
}

public class Job
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public string Type { get; set; } = string.Empty;
	public string Payload { get; set; } = "{}";
	public string Priority { get; set; } = "normal";
	public JobStatus Status { get; set; } = JobStatus.Pending;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? ScheduledAt { get; set; }
	public DateTime? StartedAt { get; set; }
	public DateTime? FinishedAt { get; set; }
	public int RetryCount { get; set; }
	public int MaxRetries { get; set; } = 3;
	public string? ErrorMessage { get; set; }
	public string CreatedBy { get; set; } = "system";
}
