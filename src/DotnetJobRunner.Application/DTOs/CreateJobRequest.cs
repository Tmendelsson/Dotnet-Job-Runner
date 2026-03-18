namespace DotnetJobRunner.Application.DTOs;

public class CreateJobRequest
{
    public string Type { get; set; } = string.Empty;
    public object? Payload { get; set; }
    public string Priority { get; set; } = "normal";
    public DateTime? RunAt { get; set; }
    public int MaxRetries { get; set; } = 3;
    public string CreatedBy { get; set; } = "api-user";
}
