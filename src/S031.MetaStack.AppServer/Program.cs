//using S031.MetaStack.Core.App;
using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Core.Logging;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.Loader;
using System.Linq;

namespace S031.MetaStack.AppServer
{
	class Program
	{
		public static async Task Main(string[] args)
		{
			var host = new HostBuilder()
				.ConfigureAppConfiguration(config => config.AddJsonFile("config.json", optional: true))
				.ConfigureLogging(factory => factory.AddFileLog())
				.ConfigureServices((context, services) => ConfigureServicesFromConfigFile(context, services))
				.UseConsoleLifetime()
				.Build();

			using (var cts = new CancellationTokenSource())
			using (host)
			{
				await host.RunAsync(cts.Token);
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

		//static void Main(string[] args)
		//{
		//	using (var cts = new CancellationTokenSource())
		//	using (IAppHost host = new AppHost("config.json"))
		//	{
		//		Console.WriteLine($"{host.AppServerName} Starting...");
		//		ConsoleExtensions.OnExit(() => 
		//		{
		//			Console.WriteLine($"{host.AppServerName} Stoping...");
		//			cts.Cancel();
		//			return true;
		//		});
		//		Console.WriteLine($"{host.AppServerName} Started press Ctrl+C for stopping it");
		//		host.Run(cts.Token);
		//		cts.Token.WaitHandle.WaitOne();
		//		Console.WriteLine($"{host.AppServerName} Stoped");
		//	}
		//}
	}
}