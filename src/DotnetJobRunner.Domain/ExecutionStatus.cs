namespace DotnetJobRunner.Domain;

/// <summary>
/// Status of a job execution (distinct from job status).
/// A job can have multiple executions, each with its own status.
/// </summary>
public enum ExecutionStatus
{
    Queued = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}
