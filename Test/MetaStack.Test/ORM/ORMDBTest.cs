using Microsoft.Extensions.Configuration;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.App;
using S031.MetaStack.Data;
using S031.MetaStack.Core.ORM;
using S031.MetaStack.ORM;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace MetaStack.Test.ORM
{
	public class ORMDBTest
	{
		private readonly string _cn;
		private readonly ILogger _logger;

		/// <summary>
		/// Required MetaStack database in sql server
		/// </summary>
		public ORMDBTest()
		{
			_logger = Program
				.GetServices()
				.GetRequiredService<ILoggerProvider>()
				.CreateLogger("ORMDBTest");

			var mdbTest = new MetaStack.Test.Data.MdbContextTest();
			_cn = mdbTest.connectionString;
			DbConnectionStringBuilder sb = new DbConnectionStringBuilder
			{
				ConnectionString = _cn
			};
			sb["Initial Catalog"] = "MetaStack";
			_cn = sb.ToString();
		}

		/// <summary>
		/// DBSchemaProviderTest, obtain <see cref="JMXSchemaProviderDB"/> and create SysCat if not exists
		/// </summary>
		[Fact]
		private void Test1()
		{

			using (MdbContext mdb = new MdbContext(_cn))
			using (JMXFactory f = JMXFactory.Create(mdb, _logger))
			{
			}
		}
		/// <summary>
		/// Create test schemas from <see cref="Resources.TestSchemas"/>
		/// </summary>
		[Fact]
		private void Test2()
		{
			SaveSchemaTestAsyncNew().GetAwaiter().GetResult();
		}

		private async Task SaveSchemaTestAsyncNew()
		{
			using (MdbContext mdb = new MdbContext(_cn))
			using (JMXFactory f = JMXFactory.Create(mdb, _logger))
			{
				var stor = f.CreateJMXRepo();
				foreach (var s in GetTestSchemas())
				{
					await stor.SaveSchemaAsync(s);
				}
			}
		}

		/// <summary>
		/// Create database objects from saved schemas
		/// </summary>
		[Fact]
		private void Test3()
		{
			SyncSchemaTestAsyncNew().GetAwaiter().GetResult();
		}

		private async Task SyncSchemaTestAsyncNew()
		{
			using (MdbContext mdb = new MdbContext(_cn))
			using (JMXFactory f = JMXFactory.Create(mdb, _logger))
			{
				var stor = f.CreateJMXRepo();
				foreach (string s in GetTestNames())
				{
					await stor.SyncSchemaAsync(s);
				}
			}
		}

		//[Fact]
		//void Test3_1()
		//{
		//	ChangeSchemaTestAsync().GetAwaiter().GetResult();
		//}
		//async Task ChangeSchemaTestAsync()
		//{
		//	using (FileLogger _logger = new FileLogger("ORMDBTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
		//	{
		//		JMXSchemaProviderDB p = JMXSchemaProviderFactory.GetProvider<JMXSchemaProviderDB>(_cn, _logger);
		//		var schema = await p.GetSchemaAsync("dbo.SysDataTypesRow");
		//		//schema.DbObjectName = new JMXObjectName("dbo", "SDTRs");
		//		schema.Attributes[12].Width = 512;
		//		await p.SaveSchemaAsync(schema);
		//		await p.SyncSchemaAsync("dbo.SysDataTypesRow");
		//	}
		//}


		/// <summary>
		/// Delete database objects created with <see cref="Test3"/>
		/// </summary>
		[Fact]
		private void Test4()
		{
			DropSchemaTestAsyncNew().GetAwaiter().GetResult();
		}

		private async Task DropSchemaTestAsyncNew()
		{
			using (MdbContext mdb = new MdbContext(_cn))
			using (JMXFactory f = JMXFactory.Create(mdb, _logger))
			{
				var stor = f.CreateJMXRepo();
				foreach (string s in GetTestNames())
				{
					await stor.DropSchemaAsync(s);
				}
			}
		}

		/// <summary>
		/// Delete SysCat from database
		/// </summary>
		[Fact]
		private void Test5()
		{
			DropDBSchemaTestAsyncNew().GetAwaiter().GetResult();
		}

		private async Task DropDBSchemaTestAsyncNew()
		{
			using (MdbContext mdb = new MdbContext(_cn))
			using (JMXFactory f = JMXFactory.Create(mdb, _logger))
			{
				await f.CreateJMXRepo().ClearCatalogAsync();
			}
		}

		[Fact]
		private void SpeedGetHashCodeTest()
		{
			using (MdbContext mdb = new MdbContext(_cn))
			{
				//string s1 = mdb.Execute<string>("SysCat.Get_TableSchema_xml",
				//	new MdbParameter("@table_name", "dbo.OrderDetails"));
				//_logger.Debug(s1);
				//_logger.Debug(JMXSchema.ParseXml(s1).ToString());
				//s1 = mdb.Execute<string>("SysCat.Get_TableSchema",
				//	new MdbParameter("@table_name", "dbo.Orders"));
				//_logger.Debug(s1);

				//_logger.Debug("Start speed test for SysCat.Get_TableSchema_xml");
				//for (int i = 0; i < 100; i++)
				//{
				//	s1 = mdb.Execute<string>("SysCat.Get_TableSchema_xml",
				//		new MdbParameter("@table_name", "dbo.OrderDetails"));
				//	var schema = JMXSchema.ParseXml(s1);
				//}
				//_logger.Debug("End speed test for SysCat.Get_TableSchema_xml");
				_logger.LogDebug("Start speed test for SysCat.Get_TableSchema");
				for (int i = 0; i < 100; i++)
				{
					var s1 = mdb.Execute<string>("SysCat.Get_TableSchema",
						new MdbParameter("@table_name", "dbo.OrderDetails"));
					var schema = JMXSchema.Parse(s1);
				}
				_logger.LogDebug("End speed test for SysCat.Get_TableSchema");
			}
		}

		[Fact]
		private void CreateFactorySpeedGTest()
		{
			var _configuration = ApplicationContext.GetConfiguration();
			var _schemaConnectInfo = _configuration.GetSection($"connectionStrings:{_configuration["appSettings:SysCatConnection"]}").Get<ConnectInfo>();
			var workConnectInfo = _configuration.GetSection($"connectionStrings:Test").Get<ConnectInfo>();
			MdbContext schemaDb = new MdbContext(_schemaConnectInfo);
			_logger.LogDebug("Start speed test for ApplicationContext.CreateJMXFactory");

			for (int i = 0; i < 10000; i++)
			{
				//using (MdbContext workDb = new MdbContext(workConnectInfo))
				//using (JMXFactory f = JMXFactory.Create(schemaDb, workDb, _logger))
				//{

				//}
				using (JMXFactory f = ApplicationContext.CreateJMXFactory("Test"))
				{

				}
			}
			_logger.LogDebug("End speed test for ApplicationContext.CreateJMXFactory");
		}

		[Fact]
		private void SaveTestData()
		{
			//foreach (var s in GetTestSchemas())
			//	System.IO.File.WriteAllText($"d:\\testData\\{s.ObjectName}.json", s.ToString());
			//foreach (var f in new DirectoryInfo("d:\\testData").GetFiles("*.json", SearchOption.AllDirectories))
			//{
			//	string s = File.ReadAllText(f.FullName);
			//	File.WriteAllText(f.FullName, s, Encoding.UTF8);
			//}
			var s = CreateTestSchema();
			_logger.LogDebug(s.ToString());
			var s1 = JMXSchema.Parse(s.ToString());
			Assert.Equal(s.ToString(), s1.ToString());
			_logger.LogDebug(s1.ToString());
		}

		[Fact]
		private void BaseDocumentTest()
		{
			BaseDocument basedocument = new BaseDocument("DealAcc");
			basedocument.Save(true);
		}

		private JMXSchema CreateTestSchema()
		{

			JMXSchema s = new JMXSchema("SysSchema")
			{
				DbObjectName = new JMXObjectName("dbo", "SysSchemas")
			};
			s.Attributes.Add(new JMXAttribute("ID") { DataType = MdbType.@int, Caption = "Identifier" });
			s.Attributes.Add(new JMXAttribute("AreaID") { DataType = MdbType.@int, Caption = "Identifier of SysArea" });
			s.Attributes.Add(new JMXAttribute("Name") { DataType = MdbType.@string, Caption = "Name of SysSchema" });
			s.PrimaryKey = new JMXPrimaryKey("PK_SysSchemas", "ID");
			s.Indexes.Add(new JMXIndex("IE1_SysSchemas", "Name"));
			var fk = new JMXForeignKey("FK1")
			{
				RefObjectName = "SysCat.SysArea",
				RefDbObjectName = new JMXObjectName("SysCat", "SysAreas")
			};
			fk.AddKeyMember("AreaID");
			fk.AddRefKeyMember("ID");
			s.ForeignKeys.Add(fk);
			return s;
		}

		private static string[] GetTestNames()
		{
			return new string[] { "dbo.Customer", "dbo.Terminal", "dbo.Contact", "dbo.Terminal2Customer",
				"dbo.Card", "dbo.PaymentState", "dbo.ErrorCode", "dbo.Request", "dbo.Payment", "dbo.PaymentStateHist" };
		}

		private static JMXSchema[] GetTestSchemas()
		{
			List<JMXSchema> l = new List<JMXSchema>();
			var rm = Resources.TestSchemas.ResourceManager;
			foreach (JMXObjectName item in GetTestNames())
			{
				var path = $@"..\..\..\orm\Resources\TestSchemas\{item.ObjectName}.json";
				l.Add(JMXSchema.Parse(File.ReadAllText(path)));
			}
			return l.ToArray();
		}

	}
}
