using System;
using System.Collections.Generic;
using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.Jobs.Payloads;
using Microsoft.Extensions.Logging;

namespace DotnetJobRunner.Application.Jobs.Handlers
{
    public class GenerateReportJobHandler(ILogger<GenerateReportJobHandler> logger) 
        : JobHandlerBase<GenerateReportPayload>
    {
        public override string JobType => "generate-report";
        public override async Task<JobExecutionResult> ExecuteAsync(
            GenerateReportPayload payload,
            JobExecutionContext context,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(payload.ReportName))
            {
                return JobExecutionResult.Failure("GenerateReport payload requires 'reportName'.");
            }

            logger.LogInformation(
                "Generating report job {JobId} for report '{ReportName}' in format '{Format}'.",
                context.JobId,
                payload.ReportName,
                payload.Format);
            await Task.Delay(700, cancellationToken);

            return JobExecutionResult.Success(
                $"Report '{payload.ReportName}' generated in format '{payload.Format}'.");
        }
    }
}
