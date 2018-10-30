using AdoNetCore.AseClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Data;
using System;
using System.Data;
using System.Data.Common;
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
			//using (MdbContext mdb = new MdbContext(cn))
			//using (var dr = mdb.GetReader("dbo.fct_select_deal_values", 
			//	"@Date", "2018-09-30".ToDateOrDefault(),
			//	"@DealType", "КредЮЛ_КК"))
			//{
			//	for (; dr.Read();)
			//	{
			//		Console.WriteLine($"{dr[0]}\t{dr[1]}");
			//		//Console.WriteLine($"{dr[0]}\t{dr[1]}\t{dr[2]}\t{dr[3]}");
			//	}
			//}
			//	DbProviderFactory dbFactory = AseClientFactory.Instance;
			//	using (DbConnection c = dbFactory.CreateConnection())
			//	using (DbCommand comm = dbFactory.CreateCommand())
			//	{
			//		c.ConnectionString = cn.ConnectionString;
			//		comm.Connection = c;
			//		//comm.CommandText = "dbo.fct_select_deal_values";
			//		//comm.CommandType = CommandType.StoredProcedure;
			//		comm.CommandText = "exec dbo.fct_select_deal_values '2018-09-30', 'КредЮЛ_КК'";
			//		comm.CommandType = CommandType.Text;
			//		//var p = comm.CreateParameter();
			//		//p.ParameterName = "@Date";
			//		//p.Value = "2018-09-30".ToDateOrDefault();
			//		//comm.Parameters.Add(p);
			//		//p = comm.CreateParameter();
			//		//p.ParameterName = "@DealType";
			//		//p.Value = "КредЮЛ_КК";
			//		//comm.Parameters.Add(p);

			//		c.Open();
			//		for (int i = 0; i < 1; i++)
			//		{
			//			using (IDataReader dr = comm.ExecuteReader())
			//			{
			//				for (; dr.Read();)
			//				{
			//					Console.WriteLine($"{dr[0]}\t{dr[1]}");
			//				}
			//			}
			//		}
			//	}
			using (var connection = new AseConnection(cn.ConnectionString))
			{
				connection.Open();
				using (var command = connection.CreateCommand())
				{
					command.CommandText = "exec dbo.fct_select_deal_values @Date, @DealType";
					command.CommandType = CommandType.Text;
					command.Parameters.AddWithValue("@Date", "2018-09-30".ToDateOrDefault());
					command.Parameters.AddWithValue("@DealType", "");
					//var p = command.CreateParameter();
					//p.ParameterName = "@Date";
					//p.AseDbType = AseDbType.SmallDateTime;
					//p.Value = "2018-09-30".ToDateOrDefault();
					//command.Parameters.Add(p);
					//p = command.CreateParameter();
					//p.AseDbType = AseDbType.VarChar;
					//p.ParameterName = "@DealType";
					//p.Value = "КредЮЛ_КК";
					//command.Parameters.Add(p);
					using (var reader = command.ExecuteReader())
					{
						Console.WriteLine($"{reader.FieldCount}");
						while (reader.Read())
						{
							Console.WriteLine($"{reader[0]}\t{reader[1]}");
						}
					}
				}
			}
	}
	}
}
