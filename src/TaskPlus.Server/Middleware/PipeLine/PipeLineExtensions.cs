using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskPlus.Server.Middleware;

namespace Microsoft.AspNetCore.Builder
{
	public static class PipeLineExtensions
	{
        public static IApplicationBuilder UsePipeLine(this IApplicationBuilder builder)
            => builder.UseMiddleware<PipeLineController>();
    }
}
