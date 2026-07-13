using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotnetJobRunner.Application.Abstractions;

namespace DotnetJobRunner.Application.Services
{
    public class JobHandlerResolver : IJobHandlerResolver
    {
        private readonly IReadOnlyDictionary<string, IJobHandler> _handlers;
        public JobHandlerResolver(IEnumerable<IJobHandler> handlers)
        {
            _handlers = handlers.ToDictionary(
            handler => Normalize(handler.JobType),
            handler => handler,
            StringComparer.OrdinalIgnoreCase);
        }
        public IReadOnlyCollection<string> SupportedJobTypes => _handlers.Keys.ToArray();
        public bool Exists(string jobType)
        {
            return _handlers.ContainsKey(Normalize(jobType));
        }
        public IJobHandler Resolve(string jobType)
        {
            var normalizedJobType = Normalize(jobType);

            if(_handlers.TryGetValue(normalizedJobType, out var handler))
            {
                return handler;
            }

            var supportedTypes = string.Join(", ", SupportedJobTypes);

            throw new InvalidOperationException(
                $"Job type '{jobType}' is not supported. Supported types: {supportedTypes}."
            );

        }

        public static string Normalize(string jobType)
        {
            return jobType.Trim().ToLowerInvariant();
        }
    }
}
