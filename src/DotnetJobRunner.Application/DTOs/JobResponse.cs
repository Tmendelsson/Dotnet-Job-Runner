using DotnetJobRunner.Domain;

namespace DotnetJobRunner.Application.DTOs;

public class JobResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public JobStatus Status { get; set; }
    public string Priority { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
    public string? ErrorMessage { get; set; }
}
