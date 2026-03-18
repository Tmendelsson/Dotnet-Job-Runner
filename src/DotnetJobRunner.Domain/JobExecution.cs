namespace DotnetJobRunner.Domain;

public class JobExecution
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid JobId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Processing;
    public string? Log { get; set; }
    public string? ErrorMessage { get; set; }
    public int Attempt { get; set; }
    public long? DurationInMs { get; set; }

    public Job? Job { get; set; }
}
