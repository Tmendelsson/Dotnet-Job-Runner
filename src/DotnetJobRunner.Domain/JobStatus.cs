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
