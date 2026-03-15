using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.DTOs;
using DotnetJobRunner.Domain;
using Microsoft.AspNetCore.Mvc;

namespace DotnetJobRunner.Api.Controllers;

[ApiController]
[Route("jobs")]
public class JobsController(IJobService jobService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobRequest request, CancellationToken cancellationToken)
    {
        var result = await jobService.CreateAsync(request, cancellationToken);
        return AcceptedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var job = await jobService.GetByIdAsync(id, cancellationToken);
        return job is null ? NotFound() : Ok(job);
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] JobStatus? status,
        [FromQuery] string? type,
        [FromQuery] string? priority,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var jobs = await jobService.ListAsync(new JobQueryRequest
        {
            Status = status,
            Type = type,
            Priority = priority,
            Page = page,
            PageSize = pageSize
        }, cancellationToken);

        return Ok(jobs);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var ok = await jobService.CancelAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/retry")]
    public async Task<IActionResult> Retry([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var ok = await jobService.RetryAsync(id, cancellationToken);
        return ok ? Accepted() : NotFound();
    }
}
