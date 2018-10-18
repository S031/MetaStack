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

			using (var host = new HostBuilder()
				.UseConsoleLifetime()
				.UseApplicationContext(configuration)
				.Build())
			{
				//await host.RunAsync(ApplicationContext.CancellationToken);
				await host.RunAsync();
			}
		}
		private static void TestConnection()
		{
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
	}
}