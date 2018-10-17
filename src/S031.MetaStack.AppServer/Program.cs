using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Data;
using S031.MetaStack.Core.Logging;
using S031.MetaStack.Core.ORM;
using S031.MetaStack.Core.Security;
using System;
using System.Data.Common;
using System.Net;
using System.Reflection;
using System.Runtime.Loader;
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
			using (var host = new HostBuilder()
				.UseConsoleLifetime()
				.ConfigureServices((context, services) => services
					.AddSingleton<ILogger>(s => logger)
					.AddSingleton<IConfiguration>(configuration)
					.AddSingleton<DbProviderFactory>(System.Data.SqlClient.SqlClientFactory.Instance)
					.AddSingleton<ILoginFactory>(new BasicLoginFactory())
					)
				.ConfigureServices((context, services) =>
					ConfigureServicesFromConfigFile(context, services))
				.UseApplicationContext()
				.Build())
			{
				//TestConnection();
				//await host.RunAsync(ApplicationContext.CancellationToken);
				await host.RunAsync();
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
		private static void TestConnection()
		{
			//var factory = System.Data.SqlClient.SqlClientFactory.Instance;
			LoadAssembly("S031.MetaStack.Core.ORM.MsSql");
			var sp = ApplicationContext.GetServices();
			var config = sp.GetService<IConfiguration>();
			var connectionName = Dns.GetHostName() == "SERGEY-WRK" ? "Test" : "BankLocal";
			var cn = config.GetSection($"connectionStrings:{connectionName}").Get<ConnectInfo>();
			var log = sp.GetService<ILogger>();
			using (MdbContext mdb = new MdbContext(cn))
			using (JMXFactory f = JMXFactory.Create(mdb, log))
			{

			}
		}
		private static Assembly LoadAssembly(string assemblyID)
		{
			Assembly a = AssemblyLoadContext.Default.LoadFromAssemblyPath(
				System.IO.Path.Combine(System.AppContext.BaseDirectory, $"{assemblyID}.dll"));
			return a;
		}
	}
}