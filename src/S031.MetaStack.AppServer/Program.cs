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
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using AdoNetCore.AseClient;

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
				TestConnection();
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
			//using (JMXFactory f = JMXFactory.Create(mdb, log))
			{
				Console.WriteLine(
					mdb.Execute<string>($"select top 1 convert(integer, ModuleId) as ModuleId from cashflow_nkn where moduleid = 45 and Date < '{DateTime.Now.ToString("yyyyMMdd")}'")
				);
			}
		}

		//const string _connectionString = "Data Source=barium,5000;Initial Catalog=workdb;User ID=LoaderF;Password=igbyltkm;charset='cp1251'";

		//public static void TestConnection()
		//{
		//	//DbProviderFactory dbFactory = DbProviderFactories.GetFactory(c);
		//	//DbProviderFactory dbFactory = DbProviderFactories.GetFactory("Sybase.Data.AseClient");
		//	DbProviderFactory dbFactory = AseClientFactory.Instance;
		//	using (DbConnection c = dbFactory.CreateConnection())
		//	using (DbCommand comm = dbFactory.CreateCommand())
		//	{
		//		c.ConnectionString = _connectionString;
		//		comm.Connection = c;
		//		comm.CommandText = "select count(*) from tuser";
		//		comm.CommandType = CommandType.Text;
		//		c.Open();
		//		for (int i = 0; i < 1; i++)
		//		{
		//			using (IDataReader dr = comm.ExecuteReader())
		//			{
		//				for (; dr.Read();)
		//				{
		//					int j = (int)dr[0];
		//					//Console.WriteLine(dr[0]);
		//				}
		//			}
		//		}
		//	}
		//}
	}
}
