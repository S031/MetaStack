using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Logging;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

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
					.AddTransient<ILogger>(s=>logger)
					.AddSingleton<IConfiguration>(configuration))
				.ConfigureServices((context, services) =>
					ConfigureServicesFromConfigFile(context, services))
				.UseApplicationContext()
				.Build())
			{
				using (var r = cts.Token.Register(ShutdownApp))
				{
					await host.RunAsync(cts.Token);
				}
			}
		}
		static void ShutdownApp()
		{
			//var log = ApplicationContext.GetServices().GetRequiredService<ILogger>();
			//(log as FileLogger)?.Dispose();
		}

		private static void ConfigureServicesFromConfigFile(HostBuilderContext host, IServiceCollection services)
		{
			var provider = services.BuildServiceProvider();
			var configuration = provider.GetService<IConfiguration>();
			var serviceList = configuration.GetSection("IAppServiceConfiguration:ImplementationList").GetChildren();
			foreach (var section in serviceList)
			{
				var options = section.Get<Core.Services.HostedServiceOptions>();
				services.AddTransient<Core.Services.HostedServiceOptions>(s => options);
				services.Add<IHostedService>(options.TypeName, options.AssemblyName);
			}
		}
	}
}