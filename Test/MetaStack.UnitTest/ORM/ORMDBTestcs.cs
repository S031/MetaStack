using S031.MetaStack.Core.ORM;
using S031.MetaStack.Core.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

namespace MetaStack.UnitTest.ORM
{
	[TestClass]
	public class ORMDBTest
	{
		string _cn;
		
		/// <summary>
		/// Required MetaStack database in sql server
		/// </summary>
		public ORMDBTest()
		{
			var mdbTest = new MetaStack.UnitTest.Data.MdbContextTest();

			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
			_cn = mdbTest.connectionString;
			DbConnectionStringBuilder sb = new DbConnectionStringBuilder
			{
				ConnectionString = _cn
			};
			sb["Initial Catalog"] = "MetaStack";
			_cn = sb.ToString();
			JMXSchemaProviderFactory.RegisterProvider<JMXSchemaProviderDB>();
		}

		/// <summary>
		/// DBSchemaProviderTest, obtain <see cref="JMXSchemaProviderDB"/> and create SysCat if not exists
		/// </summary>
		[TestMethod]
		public void Test1()
		{
			using (FileLogger _logger = new FileLogger("ORMDBTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				JMXSchemaProviderDB p = JMXSchemaProviderFactory.GetProvider<JMXSchemaProviderDB>(_cn, _logger);
				//_logger.Debug("Start speed test for obtain JMXSchemaProviderDB");
				//for (int i = 0; i < 150000; i++)
				//	p.GetSchema("dbo.SysSequence");
				////	p = JMXSchemaProviderFactory.GetProvider<JMXSchemaProviderDB>(_cn, _logger);
				//_logger.Debug("End speed test for obtain JMXSchemaProviderDB");
				//_logger.Debug(p.GetSchema("dbo.SysSequence"));

			}
		}
		/// <summary>
		/// Create test schemas from <see cref="Resources.TestSchemas"/>
		/// </summary>
		[TestMethod]
		public void Test2()
		{
			//SaveSchemaTestAsync().GetAwaiter().GetResult();
		}

		async Task SaveSchemaTestAsync()
		{
			using (FileLogger _logger = new FileLogger("ORMDBTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				JMXSchemaProviderDB p = JMXSchemaProviderFactory.GetProvider<JMXSchemaProviderDB>(_cn, _logger);
				foreach (var s in GetTestSchemas())
					await p.SaveSchemaAsync(s);
			}
		}

		/// <summary>
		/// Create database objects from saved schemas
		/// </summary>
		[TestMethod]
		public void Test3()
		{
			SyncSchemaTestAsync().GetAwaiter().GetResult();
		}

		public async Task SyncSchemaTestAsync()
		{
			using (FileLogger _logger = new FileLogger("ORMDBTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				JMXSchemaProviderDB p = JMXSchemaProviderFactory.GetProvider<JMXSchemaProviderDB>(_cn, _logger);
				//foreach( string s in getTestNames())
				//	await p.SyncSchemaAsync(s);
				//await p.SyncSchemaAsync("dbo.SysDataTypesRow");
				await p.SyncSchemaAsync("dbo.Order");
				await p.SyncSchemaAsync("dbo.OrderDetail");
			}
		}

		[TestMethod]
		public void Test3_1()
		{
			ChangeSchemaTestAsync().GetAwaiter().GetResult();
		}
		async Task ChangeSchemaTestAsync()
		{
			using (FileLogger _logger = new FileLogger("ORMDBTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				JMXSchemaProviderDB p = JMXSchemaProviderFactory.GetProvider<JMXSchemaProviderDB>(_cn, _logger);
				var schema = await p.GetSchemaAsync("dbo.SysDataTypesRow");
				//schema.DbObjectName = new JMXObjectName("dbo", "SDTRs");
				schema.Attributes[12].Width = 512;
				await p.SaveSchemaAsync(schema);
				await p.SyncSchemaAsync("dbo.SysDataTypesRow");
			}
		}


		/// <summary>
		/// Delete database objects created with <see cref="Test3"/>
		/// </summary>
		[TestMethod]
		public void Test4()
		{
			DropSchemaTestAsync().GetAwaiter().GetResult();
		}
		async Task DropSchemaTestAsync()
		{
			using (FileLogger _logger = new FileLogger("ORMDBTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				JMXSchemaProviderDB p = JMXSchemaProviderFactory.GetProvider<JMXSchemaProviderDB>(_cn, _logger);
				//foreach (string s in getTestNames())
				//	await p.DropSchemaAsync(s);
				//await p.DropSchemaAsync("dbo.SysDataTypesRow");
				await p.DropSchemaAsync("dbo.Order");
				await p.DropSchemaAsync("dbo.OrderDetail");
			}
		}
		
		/// <summary>
		/// Delete SysCat from database
		/// </summary>
		[TestMethod]
		public void Test5()
		{
			DropDBSchemaTestAsync().GetAwaiter().GetResult();
		}
		async Task DropDBSchemaTestAsync()
		{
			using (FileLogger _logger = new FileLogger("ORMDBTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				JMXSchemaProviderDB p = JMXSchemaProviderFactory.GetProvider<JMXSchemaProviderDB>(_cn, _logger);
				await p.ClearCatalogAsync();
			}
		}
		[TestMethod]
		public void speedGetHashCodeTest()
		{
			using (FileLogger _logger = new FileLogger("ORMDBTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				using (MdbContext mdb = new MdbContext(_cn, _logger))
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

		[TestMethod]
		public void saveTestData()
		{
			//foreach (var s in GetTestSchemas())
			//	System.IO.File.WriteAllText($"d:\\testData\\{s.ObjectName}.json", s.ToString());
			//foreach (var f in new DirectoryInfo("d:\\testData").GetFiles("*.json", SearchOption.AllDirectories))
			//{
			//	string s = File.ReadAllText(f.FullName);
			//	File.WriteAllText(f.FullName, s, Encoding.UTF8);
			//}
			using (FileLogger _logger = new FileLogger("ORMDBTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var s = creaateTestSchema();
				_logger.Debug(s.ToString());
				var s1 = JMXSchema.Parse(s.ToString());
				Assert.AreEqual(s.ToString(), s1.ToString());
				_logger.Debug(s1.ToString());
			}

		}

		JMXSchema creaateTestSchema()
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

		static string[] getTestNames()
		{
			return new string[] { "dbo.Customer", "dbo.Terminal", "dbo.Contact", "dbo.Terminal2Customer",
				"dbo.Card", "dbo.PaymentState", "dbo.ErrorCode", "dbo.Request", "dbo.Payment", "dbo.PaymentStateHist" };
		}
		static JMXSchema[] GetTestSchemas()
		{
			List<JMXSchema> l = new List<JMXSchema>();
			var rm = Resources.TestSchemas.ResourceManager;
			foreach (JMXObjectName item in getTestNames())
			{
				l.Add(JMXSchema.Parse(rm.GetObject(item.ObjectName) as string));
			}
			return l.ToArray();
		}

	}
}
