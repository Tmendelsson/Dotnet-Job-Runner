namespace DotnetJobRunner.Domain;

public class RecurringJobDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public string Payload { get; set; } = "{}";
    public string Priority { get; set; } = "normal";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public string CreatedBy { get; set; } = "system";
}
