namespace DotnetJobRunner.Application.DTOs;

public class CreateRecurringJobRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public object? Payload { get; set; }
    public string Priority { get; set; } = "normal";
    public string CreatedBy { get; set; } = "api-user";
}
