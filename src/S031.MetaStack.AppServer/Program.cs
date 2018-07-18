//using S031.MetaStack.Core.App;
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

namespace S031.MetaStack.AppServer
{
	class Program
	{
		public static async Task Main(string[] args)
		{
			IConfiguration configuration = new ConfigurationBuilder()
				.AddJsonFile("config.json", optional: false, reloadOnChange: true)
				.Build();

			var logSettings = configuration.GetSection("ApplicationLogSettings").Get<S031.MetaStack.Common.Logging.FileLogSettings>();
			using (var logger = new FileLogger($"S031.MetaStack.AppServer.{Environment.MachineName}", logSettings))
			{
				var host = new HostBuilder()
					.ConfigureServices((context, services) => services
						.AddSingleton<ILogger>(logger))
					.ConfigureServices((context, services) =>
						ConfigureServicesFromConfigFile(context, services))
					.UseConsoleLifetime()
					.Build();

				using (var cts = new CancellationTokenSource())
				using (host)
				{
					await host.RunAsync(cts.Token);
				}
			}
		}

		private static void ConfigureServicesFromConfigFile(HostBuilderContext host, IServiceCollection services)
		{
			Assembly a = LoadAssembly("S031.MetaStack.Services");
			foreach (Type t in a.GetExportedTypes().Where(t => typeof(IHostedService).IsAssignableFrom(t)))
				services.AddTransient(typeof(IHostedService), t);				
		}

		static Assembly LoadAssembly(string assemblyID)
		{
			Assembly a = AssemblyLoadContext.Default.LoadFromAssemblyPath(
				System.IO.Path.Combine(System.AppContext.BaseDirectory, $"{assemblyID}.dll"));
			return a;
		}
	}
}