using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetJobRunner.Application.Abstractions
{

    public sealed class JobExecutionContext
    {
        public Guid JobId { get; init; }

        public string JobType { get; init; } = string.Empty;

        public int Attempt { get; init; }

        public DateTime StartedAt { get; init; }

        public string CreatedBy { get; init; } = string.Empty;
    }
}
