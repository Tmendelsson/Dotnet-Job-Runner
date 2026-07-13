namespace DotnetJobRunner.Application.Jobs.Options;

public class JobExecutionOptions
{
    public JobTypeExecutionOptions Default { get; set; } = new();

    public Dictionary<string, JobTypeExecutionOptions> Types { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
