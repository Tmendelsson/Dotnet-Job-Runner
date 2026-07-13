using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetJobRunner.Application.Abstractions
{
   public interface IJobHandlerResolver
   {
        IReadOnlyCollection<string> SupportedJobTypes {  get; }

        bool Exists(string jobType);

        IJobHandler Resolve(string jobType);
    }
}
