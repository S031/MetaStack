using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Actions;
using S031.MetaStack.Caching;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Actions;
using S031.MetaStack.Core.Security;
using S031.MetaStack.Data;
using S031.MetaStack.Integral.Security;
using S031.MetaStack.Security;
using System.Reflection;
using System.Runtime.Loader;
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
			services.AddSingleton<IAuthorizationProvider, Security.BasicAuthorizationProvider>();
			services.AddSingleton<IUserManager, UserManager>();
			services.AddSingleton<ICacheManager, CacheManager>();
			ConfigureServicesFromConfigFile(services);
			ConfigureProvidersFromConfigFile();
			ConfigureDefaultsFromConfigFile();
			ConfigureActions();
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
		private void ConfigureProvidersFromConfigFile()
		{
			//костыль!!!
			//Remove from project references all plugins and configure publish plugins to project 
			//output folder
			//Load to publish folder all plugins with depencies (after publish plugin progect)
			var serviceList = _configuration.GetSection("Dependencies").GetChildren();
			foreach (var section in serviceList)
			{
				if (section["AssemblyPath"].IsEmpty())
					Assembly.Load(section["AssemblyName"]);
				else
					LoadAssembly(section["AssemblyPath"]);
			}
		}
		private static void ConfigureDefaultsFromConfigFile()
		{
			//костыль!!!
			//return settings from configuration
		}
		private static Assembly LoadAssembly(string assemblyID) => AssemblyLoadContext.Default.LoadFromAssemblyPath(
				System.IO.Path.Combine(System.AppContext.BaseDirectory, $"{assemblyID}.dll"));

		private static void ConfigureActions() 
			=> Actions.ActionsListInternal.CreateActionsList();
	}
}
