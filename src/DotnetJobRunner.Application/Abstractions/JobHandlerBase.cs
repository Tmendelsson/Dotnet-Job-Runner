using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetJobRunner.Application.Abstractions
{
  public abstract class JobHandlerBase<TPayload> : IJobHandler<TPayload>
  {
    public abstract string JobType { get; }
    public Type PayloadType => typeof(TPayload);
    public abstract Task<JobExecutionResult> ExecuteAsync(
        TPayload payload,
        JobExecutionContext context,
        CancellationToken cancellationToken);
    async Task<JobExecutionResult> IJobHandler.ExecuteAsync(
        object payload,
        JobExecutionContext context,
        CancellationToken cancellationToken)
    {
      if (payload is not TPayload typedPayload)
      {
        throw new ArgumentException($"Invalid payload type. Expected {typeof(TPayload).Name}, but received {payload.GetType().Name}.");
      }
      return await ExecuteAsync(typedPayload, context, cancellationToken);
    }
  }
}
