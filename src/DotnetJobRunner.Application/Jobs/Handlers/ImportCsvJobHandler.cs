using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.Jobs.Payloads;
using Microsoft.Extensions.Logging;

namespace DotnetJobRunner.Application.Jobs.Handlers
{
    public class ImportCsvJobHandler(ILogger<ImportCsvJobHandler> logger) 
        : JobHandlerBase<ImportCsvPayload>
    {
        public override string JobType => "import-csv";
        public override async Task<JobExecutionResult> ExecuteAsync(
            ImportCsvPayload payload,
            JobExecutionContext context,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(payload.FileName))
            {
                return JobExecutionResult.Failure("ImportCsv payload requires 'fileName'.");
            }
            logger.LogInformation(
                "Importing CSV job {JobId} from file '{FileName}'.",
                context.JobId,
                payload.FileName);
            await Task.Delay(500, cancellationToken);
            return JobExecutionResult.Success(
                $"CSV file '{payload.FileName}' imported successfully.");
        }
    }
}
