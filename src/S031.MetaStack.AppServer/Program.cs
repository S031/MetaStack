using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Data;
using System;
using System.Threading.Tasks;

namespace S031.MetaStack.AppServer
{
	class Program
	{
		/// <summary>
		////Костыль!!! Исключить из ссылок рантайм dll (aka \ORM\*.dll
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
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
				//TestConnection();
				await host.RunAsync();
			}
		}
		private static void TestConnection()
		{
			var sp = ApplicationContext.GetServices();
			var config = sp.GetService<IConfiguration>();
			var connectionName = config["appSettings:defaultConnection"];
			var cn = config.GetSection($"connectionStrings:{connectionName}").Get<ConnectInfo>();
			var log = sp.GetService<ILogger>();
			using (MdbContext mdb = new MdbContext(cn))
			using (var dr = mdb.GetReader("select Top 1000 * from tuser"))
			{
				for (; dr.Read();)
				{
					Console.WriteLine($"{dr[0]}\t{dr[1]}\t{dr[2]}\t{dr[3]}");
				}
			}
		}
	}
}
