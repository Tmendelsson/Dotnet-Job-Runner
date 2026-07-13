using Hangfire.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetJobRunner.Application.Abstractions
{
    public sealed class JobExecutionResult { 
        private JobExecutionResult(bool succeeded, string log, string? errorMessage)
        {
            Succeeded = succeeded;
            Log = log;
            ErrorMessage = errorMessage;
        }

        public bool Succeeded { get; }

        public string Log { get; }

        public string? ErrorMessage { get; }

        public static JobExecutionResult Success(string log)
        {
            return new JobExecutionResult(true, log, null);
        }

        public static JobExecutionResult Failure(string errorMessage, string? log = null)
        {
            return new JobExecutionResult(false, log ?? errorMessage, errorMessage);
        }
    }
}
