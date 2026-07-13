using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetJobRunner.Api.Controllers;

[ApiController]
[Authorize]
[Route("recurring-jobs")]
public class RecurringJobsController(IJobService jobService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Create([FromBody] CreateRecurringJobRequest request, CancellationToken cancellationToken)
    {
        var result = await jobService.CreateRecurringAsync(request, cancellationToken);
        return Accepted(result);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Operator,Viewer")]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var jobs = await jobService.ListRecurringAsync(cancellationToken);
        return Ok(jobs);
    }

    [HttpPatch("{id:guid}/enable")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Enable([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var ok = await jobService.EnableRecurringAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpPatch("{id:guid}/disable")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Disable([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var ok = await jobService.DisableRecurringAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var ok = await jobService.DeleteRecurringAsync(id, cancellationToken);
        return ok ? NoContent() : NotFound();
    }
}
