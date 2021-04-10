using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Actions;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Actions;
using S031.MetaStack.Core.Security;
using S031.MetaStack.Data;
using S031.MetaStack.Security;
using System.Reflection;
using System.Threading;

namespace S031.MetaStack.AppServer
{
	public class Startup : IStartup
	{
		private IConfiguration _configuration;
		void IStartup.Configure(HostBuilderContext context)
		{
			//throw new NotImplementedException
			_configuration = context.Configuration;
		}

		void IStartup.ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton(p => p.GetRequiredService<ILoggerProvider>()
				.CreateLogger(Assembly.GetEntryAssembly().GetWorkName()));
			services.AddSingleton<IMdbContextFactory, MdbContextFactory>();
			services.AddTransient<IActionManager, ActionManager>();
			services.AddSingleton<ILoginProvider, BasicLoginProvider>();
			services.AddSingleton<PipeQueue, PipeQueue>();
			ConfigureServicesFromConfigFile(services);
		}
		
		private void ConfigureServicesFromConfigFile(IServiceCollection services)
		{
			var serviceList = _configuration.GetSection("IAppServiceConfiguration:ImplementationList").GetChildren();
			foreach (var section in serviceList)
			{
				var options = section.Get<Core.Services.HostedServiceOptions>();
				services.AddScoped(s => options);
				services.Add<IHostedService>(options.TypeName, options.AssemblyName, options.ServiceLifetime);
			}
		}
	}
}
