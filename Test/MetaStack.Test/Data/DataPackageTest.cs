using Xunit;
using S031.MetaStack.Common.Logging;
using System.Collections.Generic;
using System;
using System.Text;
using S031.MetaStack.Core.Data;
using MessagePack;
using S031.MetaStack.Core.Json;

namespace MetaStack.Test.Data
{
	public class DataPackageTest
	{
		public DataPackageTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}
		[Fact]
		void ctorTest()
		{
			using (FileLog l = new FileLog("DataPackageTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				DataPackage p = new DataPackage(new string[] { "Col1.long", "Col2.string.255", "Col3.datetime.10", "Col4.Guid.34" },
					new object[] { 999999999999, "Property one", DateTime.Now, Guid.NewGuid() });
				p.GoDataTop();
				p.Read();
				l.Debug(p.GetRowJSON());
				l.Debug(p.ToString(TsExportFormat.JSON));

				p = new DataPackage(new string[] { "Col1.int", "Col2.string.255", "Col3.datetime.10", "Col4.Guid.34", "Col5.object" });
				for (int i = 125; i < 135; i++)
				{
					p.AddNew();
					p["Col1"] = i;
					p["Col2"] = $"Строка # {i}";
					p["Col3"] = DateTime.Now.AddDays(i);
					p["Col4"] = Guid.NewGuid();
					testClass c = new testClass
					{
						ID = i,
						Name = (string)p["Col2"]
					};
					c.ItemList.Add(p["Col4"].ToString(), p["Col4"]);
					p["Col5"] = c;
					p.Update();
				}
				p.GoDataTop();
				for (; p.Read();)
				{
					l.Debug(p.GetRowJSON());
				}
				l.Debug(p.ToString(TsExportFormat.JSON));

				p = new DataPackage("Col1", 999999999999, "Col2", "Свойство 1", "Col3", DateTime.Now, "Col4", Guid.NewGuid());
				p.GoDataTop();
				p.Read();
				l.Debug(p.GetRowJSON());
				l.Debug(p.ToString(TsExportFormat.JSON));
			}
		}

		[Fact]
		void speedTest()
		{
			using (FileLog l = new FileLog("DataPackageTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			using (DataPackage p = new DataPackage(new string[] { "Col1.int", "Col2.string.255", "Col3.datetime.10", "Col4.Guid.34", "Col5.object" }))
			{
				l.Debug("SpeedTest Start");
				int i = 0;
				for (i = 0; i < 1000000; i++)
				{
					p.AddNew();
					p["Col1"] = i;
					p["Col2"] = $"Строка # {i}";
					p["Col3"] = DateTime.Now.AddDays(i);
					p["Col4"] = Guid.NewGuid();
					//без сериализации работает в 1.5 раза быстрееp
					//p["Col5"] = null;
					p["Col5"] = new testClass();
					p.Update();
				}
				l.Debug($"SpeedTest Finish {i} rows added");
				p.GoDataTop();
				i = 0;
				for (; p.Read();)
				{
					i++;
				}
				l.Debug($"SpeedTest Finish {i} rows readed");

			}
		}
		[Fact]
		void byteArrayTest()
		{
			using (FileLog l = new FileLog("DataPackageTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				DataPackage p = new DataPackage(new string[] { "Col1.int", "Col2.string.255", "Col3.datetime.10", "Col4.Guid.34", "Col5.object" });
				p.Headers.Add("Username", "Сергей");
				p.Headers.Add("Password", "1234567T");
				p.Headers.Add("Sign", UnicodeEncoding.UTF8.GetBytes("Сергей"));
				p.UpdateHeaders();
				int i = 0;
				for (i = 0; i < 5; i++)
				{
					p.AddNew();
					p["Col1"] = i;
					p["Col2"] = $"Строка # {i}";
					p["Col3"] = DateTime.Now.AddDays(i);
					p["Col4"] = Guid.NewGuid();
					//без сериализации работает в 1.5 раза быстрееp
					//p["Col5"] = null;
					p["Col5"] = new testClass() { ID = i, Name = (string)p["Col2"] };
					p.Update();
				}
				l.Debug($"byteArrayTest source {i} rows added");
				l.Debug(p.ToString(TsExportFormat.JSON));
				int hash = p.ToString(TsExportFormat.JSON).GetHashCode();
				p = new DataPackage(p.ToArray());
				l.Debug($"byteArrayTest after conversion");
				l.Debug(p.ToString(TsExportFormat.JSON));
				Assert.True(hash == p.ToString(TsExportFormat.JSON).GetHashCode());
			}
		}

		[Fact]
		void parseTest()
		{
			using (FileLog l = new FileLog("DataPackageTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				DataPackage p = new DataPackage(new string[] { "Col1.int", "Col2.string.255", "Col3.datetime.10", "Col4.Guid.34", "Col5.object" });
				p.Headers.Add("Username", "Сергей");
				p.Headers.Add("Password", "1234567T");
				p.UpdateHeaders();
				int i = 0;
				for (i = 0; i < 5; i++)
				{
					p.AddNew();
					p["Col1"] = i;
					p["Col2"] = $"Строка # {i}";
					p["Col3"] = DateTime.Now.AddDays(i);
					p["Col4"] = Guid.NewGuid();
					//без сериализации работает в 1.5 раза быстрееp
					//p["Col5"] = null;
					p["Col5"] = new testClass() { ID = i, Name = (string)p["Col2"] };
					p.Update();
				}
				l.Debug($"parseTest source {i} rows added");
				l.Debug(p.ToString(TsExportFormat.JSON));
				int hash = p.ToString(TsExportFormat.JSON).GetHashCode();
				p = DataPackage.Parse(p.ToString(TsExportFormat.JSON));
				l.Debug("parseTest after conversion");
				l.Debug(p.ToString(TsExportFormat.JSON));
				Assert.True(hash == p.ToString(TsExportFormat.JSON).GetHashCode());

				string source = @"{'string1':'value','integer2':99,'datetime3':'2017-05-23T00:00:00','time4':'22:00:00'}";
				p = DataPackage.Parse(512, source);
				l.Debug(p.ToString(TsExportFormat.JSON));
			}
		}

		[Fact]
		void headerTest()
		{
			using (FileLog l = new FileLog("DataPackageTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				DataPackage p = new DataPackage(16, new string[] { "Col1.int", "Col2.string.255", "Col3.datetime.10", "Col4.Guid.34", "Col5.object" }, null);
				p.Headers.Add("Username", "Сергей");
				p.Headers.Add("Password", "1234567T");
				p.Headers.Add("Sign", UnicodeEncoding.UTF8.GetBytes("Сергей"));
				try
				{
					p.UpdateHeaders();
				}
				catch (OverflowException oe)
				{
					l.Debug(oe.Message);
				}
				p = new DataPackage(512, new string[] { "Col1.int", "Col2.string.255", "Col3.datetime.10", "Col4.Guid.34", "Col5.object" }, null);
				p.Headers.Add("Username", "Сергей");
				p.Headers.Add("Password", "1234567T");
				p.Headers.Add("Sign", UnicodeEncoding.UTF8.GetBytes("Сергей"));
				p.UpdateHeaders();
				int i = 0;
				for (i = 0; i < 5; i++)
				{
					p.AddNew();
					p["Col1"] = i;
					p["Col2"] = $"Строка # {i}";
					p["Col3"] = DateTime.Now.AddDays(i);
					p["Col4"] = Guid.NewGuid();
					//без сериализации работает в 1.5 раза быстрееp
					//p["Col5"] = null;
					p["Col5"] = new testClass() { ID = i, Name = (string)p["Col2"] };
					p.Update();
				}
				l.Debug($"headerTest source {i} rows added");
				l.Debug(p.ToString(TsExportFormat.JSON));
			}
		}
		[Fact]
		void writeDataTest()
		{
			using (FileLog l = new FileLog("DataPackageTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				DataPackage p = new DataPackage(new string[] { "Col1.int", "Col2.string.255", "Col3.datetime.10", "Col4.Guid.34", "Col5.object" });
				p.Headers.Add("Username", "Сергей");
				p.Headers.Add("Password", "1234567T");
				p.Headers.Add("Sign", UnicodeEncoding.UTF8.GetBytes("Сергей"));
				p.UpdateHeaders();
				int i = 0;
				for (i = 0; i < 5; i++)
				{
					p.AddNew();
					p["Col1"] = i;
					p["Col2"] = $"Строка # {i}";
					p["Col3"] = DateTime.Now.AddDays(i);
					p["Col4"] = Guid.NewGuid();
					//без сериализации работает в 1.5 раза быстрееp
					//p["Col5"] = null;
					p["Col5"] = new testClass() { ID = i, Name = (string)p["Col2"] };
					p.Update();
				}
				l.Debug($"writeDataTest source {i} rows added");
				l.Debug(p.ToString(TsExportFormat.JSON));
				int hash = p.ToString(TsExportFormat.JSON).GetHashCode();
				var p1 = DataPackage.WriteData(p);
				l.Debug($"writeDataTest after conversion");
				l.Debug(p.ToString(TsExportFormat.JSON));
				Assert.True(hash == p.ToString(TsExportFormat.JSON).GetHashCode());
			}
		}
		[Fact]
		void ToDataTableTest()
		{
			using (FileLog l = new FileLog("DataPackageTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				DataPackage p = new DataPackage(new string[] { "Col1.int", "Col2.string.255", "Col3.datetime.10", "Col4.Guid.34", "Col5.object" });
				int i = 0;
				for (i = 0; i < 5; i++)
				{
					p.AddNew();
					p["Col1"] = i;
					p["Col2"] = $"Строка # {i}";
					p["Col3"] = DateTime.Now.AddDays(i);
					p["Col4"] = Guid.NewGuid();
					//без сериализации работает в 1.5 раза быстрееp
					//p["Col5"] = null;
					p["Col5"] = new testClass() { ID = i, Name = (string)p["Col2"] };
					p.Update();
				}
				l.Debug($"writeDataTest source {i} rows added");
				var t = p.ToDataTable();
				DisplayData(t, l);				
			}
		}

		[Fact]
		void serializationSpeedTest()
		{
			using (FileLog l = new FileLog("DataPackageTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				l.Debug("SpeedTest MessagePackSerializer Start");
				int i = 0;
				for (i = 0; i < 1000000; i++)
				{
					testClass test = new testClass() { ID = i, Name = $"Item {i}" };
					test.ItemList.Add("Item {i}", i);
					var data = MessagePackSerializer.Typeless.Serialize(test);
					test = (testClass)MessagePackSerializer.Typeless.Deserialize(data);
				}
				l.Debug("SpeedTest MessagePackSerializer Finish");
				l.Debug("SpeedTest JSONSerializer Start");
				for (i = 0; i < 1000000; i++)
				{
					testClass test = new testClass() { ID = i, Name = $"Item {i}" };
					test.ItemList.Add("Item {i}", i);
					var data = JSONExtensions.SerializeObject(test);
					JSONExtensions.DeserializeObject(data);
				}
				l.Debug("SpeedTest JSOSerializer Finish");
			}
		}
		private class testClass
		{
			public testClass()
			{
				ItemList = new Dictionary<string, object>();
			}
			public int ID { get; set; }
			public string Name { get; set; }
			public Dictionary<string, object> ItemList { get; set; }
		}
		private static void DisplayData(System.Data.DataTable table, FileLog l)
		{
			StringBuilder sb = new StringBuilder();
			foreach (System.Data.DataRow row in table.Rows)
			{
				foreach (System.Data.DataColumn col in table.Columns)
				{
					sb.AppendFormat("{0} = {1}\t", col.ColumnName, row[col]);
				}
				sb.AppendLine();
			}
			l.Debug(sb.ToString());
		}
	}
}
