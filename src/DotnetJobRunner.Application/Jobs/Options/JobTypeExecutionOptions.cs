namespace DotnetJobRunner.Application.Jobs.Options;

public class JobTypeExecutionOptions
{
    public int MaxConcurrency { get; set; } = 10;

    public int RateLimitPerMinute { get; set; }

    public int RateLimitWindowInSeconds { get; set; } = 60;
}
