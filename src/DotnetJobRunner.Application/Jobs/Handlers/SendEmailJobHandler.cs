using DotnetJobRunner.Application.Abstractions;
using DotnetJobRunner.Application.Jobs.Payloads;
using Microsoft.Extensions.Logging;

namespace DotnetJobRunner.Application.Jobs.Handlers
{
    public class SendEmailJobHandler(ILogger<SendEmailJobHandler> logger) 
        : JobHandlerBase<SendEmailPayload>
    {
        public override string JobType =>  "send-email";

        public override async Task<JobExecutionResult> ExecuteAsync(
            SendEmailPayload payload,
            JobExecutionContext context,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(payload.To))
            {
                return JobExecutionResult.Failure("SendEmail payload requires 'to' .");
            }

            if (string.IsNullOrWhiteSpace(payload.Subject)) 
            {
                return JobExecutionResult.Failure("SendEmail payload requires 'subject' .");
            }

            logger.LogInformation(
                "Sending email job {JobId} to {To} with subject {Subject}.",
                context.JobId,
                payload.To,
                payload.Subject);

            await Task.Delay(350, cancellationToken);

            return JobExecutionResult.Success(
                $"Email sent to '{payload.To}' with subject '{payload.Subject}'.");
        }
    }
}
