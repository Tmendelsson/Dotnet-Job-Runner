using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.Jobs.Payloads;
using Microsoft.Extensions.Logging;

namespace DotnetJobRunner.Application.Jobs.Handlers;

public class SyncCustomersJobHandler(ILogger<SyncCustomersJobHandler> logger)
    : JobHandlerBase<SyncCustomersPayload>
{
    public override string JobType => "sync-customers";

    public override async Task<JobExecutionResult> ExecuteAsync(
        SyncCustomersPayload payload,
        JobExecutionContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(payload.Source))
        {
            return JobExecutionResult.Failure("SyncCustomers payload requires 'source'.");
        }

        logger.LogInformation(
            "Syncing customers job {JobId} from source {Source}.",
            context.JobId,
            payload.Source);

        await Task.Delay(650, cancellationToken);

        return JobExecutionResult.Success(
            $"Customer sync completed from source '{payload.Source}'.");
    }
}