using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using S031.MetaStack.Core.Logging;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.Loader;
using System.Linq;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Core.App;

namespace S031.MetaStack.AppServer
{
	class Program
	{
		public static async Task Main(string[] args)
		{
			IConfiguration configuration = new ConfigurationBuilder()
				.AddJsonFile("config.json", optional: false, reloadOnChange: true)
				.Build();

			var logSettings = configuration.GetSection("ApplicationLogSettings").Get<Common.Logging.FileLogSettings>();
			using (var logger = new FileLogger($"S031.MetaStack.AppServer.{Environment.MachineName}", logSettings))
			using (var cts = new CancellationTokenSource())
			using (var host = new HostBuilder()
				.UseConsoleLifetime()
				.ConfigureServices((context, services) => services
					.AddTransient<ILogger>(s => logger)
					.AddSingleton<IConfiguration>(configuration))
				.ConfigureServices((context, services) =>
					ConfigureServicesFromConfigFile(context, services))
				.UseApplicationContext()
				.Build())
			{
				await host.RunAsync(cts.Token);
			}
		}


		private static void ConfigureServicesFromConfigFile(HostBuilderContext host, IServiceCollection services)
		{
			var provider = services.BuildServiceProvider();
			var configuration = provider.GetService<IConfiguration>();
			var serviceList = configuration.GetSection("IAppServiceConfiguration:ImplementationList").GetChildren();
			foreach (var section in serviceList)
			{
				var options = section.Get<Core.Services.HostedServiceOptions>();
				services.AddScoped<Core.Services.HostedServiceOptions>(s => options);
				using (var scopeProvider = provider.CreateScope())
				{
					Assembly a = LoadAssembly(options.AssemblyName);
					services.AddSingleton(typeof(IHostedService), a.GetType(options.TypeName));
				}
			}
		}

		static Assembly LoadAssembly(string assemblyID)
		{
			Assembly a = AssemblyLoadContext.Default.LoadFromAssemblyPath(
				System.IO.Path.Combine(System.AppContext.BaseDirectory, $"{assemblyID}.dll"));
			return a;
		}
	}
}