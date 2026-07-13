using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetJobRunner.Application.Jobs.Payloads
{
    public class GenerateReportPayload
    {
        public string ReportName { get; set; } = string.Empty;

        public string Format { get; set; } = "pdf";
    }
}
