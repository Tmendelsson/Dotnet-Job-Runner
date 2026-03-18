using DotnetJobRunner.Domain;

namespace DotnetJobRunner.Application.DTOs;

public class JobQueryRequest
{
    public JobStatus? Status { get; set; }
    public string? Type { get; set; }
    public string? Priority { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
