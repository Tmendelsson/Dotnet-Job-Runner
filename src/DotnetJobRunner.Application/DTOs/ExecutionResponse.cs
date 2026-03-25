using DotnetJobRunner.Domain;

namespace DotnetJobRunner.Application.DTOs;

public class ExecutionResponse
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public int Attempt { get; set; }
    public ExecutionStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public long? DurationInMs { get; set; }
    public string? Log { get; set; }
    public string? ErrorMessage { get; set; }
}
