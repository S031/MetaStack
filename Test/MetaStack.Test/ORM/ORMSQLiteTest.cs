using S031.MetaStack.Core.ORM;
using S031.MetaStack.Core.Logging;
using Xunit;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Data;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Common.Logging;
using System.Data.Common;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace MetaStack.Test.ORM
{
	public class ORMSQLiteTest
	{
		const string _cn = @"Data Source=D:\Source\Repos\Data\SQLite\MetaStack\SysCat.db; Mode=Memory; Cache=Shared";
		readonly ConnectInfo _ci;

		/// <summary>
		/// Required MetaStack database in sql server
		/// </summary>
		public ORMSQLiteTest()
		{
			Program.ConfigureTests();
			_ci = new ConnectInfo("System.Data.Sqlite", _cn);
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}

		/// <summary>
		/// DBSchemaProviderTest, obtain <see cref="JMXSchemaProviderDB"/> and create SysCat if not exists
		/// </summary>
		[Fact]
		void Test1()
		{
			using (FileLogger _logger = new FileLogger("ORMSQLiteTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			using (MdbContext mdb = new MdbContext(_ci))
			using (JMXFactory f = JMXFactory.Create(mdb, _logger))
			{
			}
		}
		/// <summary>
		/// Create test schemas from <see cref="Resources.TestSchemas"/>
		/// </summary>
		[Fact]
		void Test2()
		{
			SaveSchemaTestAsyncNew().GetAwaiter().GetResult();
		}

		async Task SaveSchemaTestAsyncNew()
		{
			using (FileLogger _logger = new FileLogger("ORMSQLiteTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			using (MdbContext mdb = new MdbContext(_ci))
			using (JMXFactory f = JMXFactory.Create(mdb, _logger))
			{
				var stor = f.CreateJMXRepo();
				foreach (var s in GetTestSchemas())
					await stor.SaveSchemaAsync(s);

			}
		}

		/// <summary>
		/// Create database objects from saved schemas
		/// </summary>
		[Fact]
		void Test3()
		{
			SyncSchemaTestAsyncNew().GetAwaiter().GetResult();
		}

		async Task SyncSchemaTestAsyncNew()
		{
			using (FileLogger _logger = new FileLogger("ORMSQLiteTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			using (MdbContext mdb = new MdbContext(_cn))
			using (JMXFactory f = JMXFactory.Create(mdb, _logger))
			{
				var stor = f.CreateJMXRepo();
				foreach (string s in GetTestNames())
					await stor.SyncSchemaAsync(s);
			}
		}

		//[Fact]
		//void Test3_1()
		//{
		//	ChangeSchemaTestAsync().GetAwaiter().GetResult();
		//}
		//async Task ChangeSchemaTestAsync()
		//{
		//	using (FileLogger _logger = new FileLogger("ORMSQLiteTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
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
		void Test4()
		{
			DropSchemaTestAsyncNew().GetAwaiter().GetResult();
		}
		async Task DropSchemaTestAsyncNew()
		{
			using (FileLogger _logger = new FileLogger("ORMSQLiteTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			using (MdbContext mdb = new MdbContext(_ci))
			using (JMXFactory f = JMXFactory.Create(mdb, _logger))
			{
				var stor = f.CreateJMXRepo();
				foreach (string s in GetTestNames())
					await stor.DropSchemaAsync(s);
			}
		}

		/// <summary>
		/// Delete SysCat from database
		/// </summary>
		[Fact]
		void Test5()
		{
			DropDBSchemaTestAsyncNew().GetAwaiter().GetResult();
		}
		async Task DropDBSchemaTestAsyncNew()
		{
			using (FileLogger _logger = new FileLogger("ORMSQLiteTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			using (MdbContext mdb = new MdbContext(_ci))
			using (JMXFactory f = JMXFactory.Create(mdb, _logger))
			{
				await f.CreateJMXRepo().ClearCatalogAsync();
			}
		}

		[Fact]
		void SpeedGetHashCodeTest()
		{
			using (FileLogger _logger = new FileLogger("ORMSQLiteTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				using (MdbContext mdb = new MdbContext(_ci))
				{
					string s1 = mdb.Execute<string>("SysCat.Get_TableSchema_xml",
						new MdbParameter("@table_name", "dbo.OrderDetails"));
					_logger.Debug(s1);
					_logger.Debug(JMXSchema.ParseXml(s1).ToString());
					s1 = mdb.Execute<string>("SysCat.Get_TableSchema",
						new MdbParameter("@table_name", "dbo.Orders"));
					_logger.Debug(s1);

					_logger.Debug("Start speed test for SysCat.Get_TableSchema_xml");
					for (int i = 0; i < 100; i++)
					{
						s1 = mdb.Execute<string>("SysCat.Get_TableSchema_xml",
							new MdbParameter("@table_name", "dbo.OrderDetails"));
						var schema = JMXSchema.ParseXml(s1);
					}
					_logger.Debug("End speed test for SysCat.Get_TableSchema_xml");
					_logger.Debug("Start speed test for SysCat.Get_TableSchema");
					for (int i = 0; i < 100; i++)
					{
						s1 = mdb.Execute<string>("SysCat.Get_TableSchema",
							new MdbParameter("@table_name", "dbo.OrderDetails"));
						var schema = JMXSchema.Parse(s1);
					}
					_logger.Debug("End speed test for SysCat.Get_TableSchema");
				}

			}
		}

		[Fact]
		void SaveTestData()
		{
			//foreach (var s in GetTestSchemas())
			//	System.IO.File.WriteAllText($"d:\\testData\\{s.ObjectName}.json", s.ToString());
			//foreach (var f in new DirectoryInfo("d:\\testData").GetFiles("*.json", SearchOption.AllDirectories))
			//{
			//	string s = File.ReadAllText(f.FullName);
			//	File.WriteAllText(f.FullName, s, Encoding.UTF8);
			//}
			using (FileLogger _logger = new FileLogger("ORMSQLiteTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var s = CreateTestSchema();
				_logger.Debug(s.ToString());
				var s1 = JMXSchema.Parse(s.ToString());
				Assert.Equal(s.ToString(), s1.ToString());
				_logger.Debug(s1.ToString());
			}

		}

		JMXSchema CreateTestSchema()
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

		static string[] GetTestNames()
		{
			return new string[] { "dbo.Customer", "dbo.Terminal", "dbo.Contact", "dbo.Terminal2Customer",
				"dbo.Card", "dbo.PaymentState", "dbo.ErrorCode", "dbo.Request", "dbo.Payment", "dbo.PaymentStateHist" };
		}
		static JMXSchema[] GetTestSchemas()
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
