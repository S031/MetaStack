using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using System.Reflection;

namespace S031.MetaStack.AppServer
{
	public class Startup : IStartup
	{
		void IStartup.Configure(HostBuilderContext context)
		{
			//throw new NotImplementedException
		}

		void IStartup.ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton(p => p.GetRequiredService<ILoggerProvider>()
				.CreateLogger(Assembly.GetEntryAssembly().GetWorkName()));
		}
	}
}
