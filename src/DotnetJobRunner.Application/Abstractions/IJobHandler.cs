using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetJobRunner.Application.Abstractions
{
    public interface IJobHandler
    {
        string JobType { get; }

        Type PayloadType { get; }

        Task<JobExecutionResult> ExecuteAsync(
            object payload,
            JobExecutionContext context,
            CancellationToken cancellationToken);
    }

    public interface IJobHandler<TPayload> : IJobHandler
    {
        Task<JobExecutionResult> ExecuteAsync(
            TPayload payload,
            JobExecutionContext context,
            CancellationToken cancellationToken);
    }
}
