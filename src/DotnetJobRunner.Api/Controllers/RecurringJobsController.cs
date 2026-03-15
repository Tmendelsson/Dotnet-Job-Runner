using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DotnetJobRunner.Api.Controllers;

[ApiController]
[Route("recurring-jobs")]
public class RecurringJobsController(IJobService jobService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecurringJobRequest request, CancellationToken cancellationToken)
    {
        var result = await jobService.CreateRecurringAsync(request, cancellationToken);
        return Accepted(result);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var jobs = await jobService.ListRecurringAsync(cancellationToken);
        return Ok(jobs);
    }

    [HttpPatch("{id:guid}/enable")]
    public async Task<IActionResult> Enable([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var ok = await jobService.EnableRecurringAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpPatch("{id:guid}/disable")]
    public async Task<IActionResult> Disable([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var ok = await jobService.DisableRecurringAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var ok = await jobService.DeleteRecurringAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}
