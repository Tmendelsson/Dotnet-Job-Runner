namespace DotnetJobRunner.Domain;

/// <summary>
/// Priority levels for jobs.
/// </summary>
public enum JobPriority
{
    Low = 0,
    Normal = 1,
    High = 2
}

/// <summary>
/// Helper extension methods for JobPriority.
/// </summary>
public static class JobPriorityExtensions
{
    public static JobPriority Parse(string? value)
    {
        return (value?.Trim().ToLowerInvariant()) switch
        {
            "low" => JobPriority.Low,
            "high" => JobPriority.High,
            _ => JobPriority.Normal
        };
    }

    public static string ToString(JobPriority priority)
    {
        return priority switch
        {
            JobPriority.Low => "low",
            JobPriority.High => "high",
            _ => "normal"
        };
    }
}
