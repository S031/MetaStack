using MessagePack;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.Data;
using S031.MetaStack.Core.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MetaStack.Test.Data
{
	public class DataPackageTest
	{
		public DataPackageTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}
		[Fact]
		private void CtorTest()
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
					TestClass c = new TestClass
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
		private void SpeedTest()
		{
			using (FileLog l = new FileLog("DataPackageTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				int i = 0;
				int count = 10_000;
				int loopCount = 1_000;
				DataPackage p = GetTestData(count, false, false);
				l.Debug("SpeedTest Start");
				for (i = 0; i < loopCount; i++)
					p = GetTestData(count, false, false);
				l.Debug($"SpeedTest Finish {i} packages with {count} rows");
				p.GoDataTop();
				i = 0;
				for (; p.Read();)
				{
					i++;
				}
				l.Debug($"SpeedTest Finish {i} rows readed");
				byte[] data =  p.ToArray();
				for (i = 0; i < loopCount; i++)
					_ = p.ToArray();
				l.Debug($"SpeedTest Finish {i} serialize with {count} rows");
				for (i = 0; i < loopCount; i++)
					p = new DataPackage(data);
				l.Debug($"SpeedTest Finish {i} deserialize with {count} rows");
			}
		}

		private static DataPackage GetTestData(int rowsCount = 5, bool withHeader = false, bool withObjectData = false)
		{
			DataPackage p = new DataPackage(new string[] { "Col1.int", "Col2.string.255", "Col3.datetime.10", "Col4.Guid.34", "Col5.object" });
			if (withHeader)
				p.SetHeader("Username", "Сергей")
				.SetHeader("Password", "1234567T")
				.SetHeader("Sign", UnicodeEncoding.UTF8.GetBytes("Сергей"))
				.UpdateHeaders();

			int i = 0;
			for (i = 0; i < rowsCount; i++)
			{
				p.AddNew()
					.SetValue(0, i)
					.SetValue(1, $"Строка # {new string('x', i % 100)}")
					.SetValue(2, DateTime.Now.AddDays(i))
					.SetValue(3, Guid.NewGuid());
				
				if (!withObjectData)
					p[4] = null;
				else
					p[4] = new TestClass() { ID = i, Name = (string)p[1] };
				p.Update();
			}
			return p;
		}

		[Fact]
		private void ByteArrayTest()
		{
			using (FileLog l = new FileLog("DataPackageTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				int i = 5;
				DataPackage p = GetTestData(i, true, true);
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
		private void ParseTest()
		{
			using (FileLog l = new FileLog("DataPackageTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				int i = 5;
				DataPackage p = GetTestData(i, true, false);
				l.Debug($"parseTest source {i} rows added");
				l.Debug(p.ToString(TsExportFormat.JSON));
				int hash = p.ToString(TsExportFormat.JSON).GetHashCode();
				p = DataPackage.Parse(p.ToString(TsExportFormat.JSON));
				l.Debug("parseTest after conversion");
				l.Debug(p.ToString(TsExportFormat.JSON));
				Assert.True(hash == p.ToString(TsExportFormat.JSON).GetHashCode());

				string source = "{\"string1\":\"value\",\"integer2\":99,\"datetime3\":\"2017-05-23T00:00:00\",\"time4\":\"22:00:00\"}";
				p = DataPackage.Parse(512, source);
				l.Debug(p.ToString(TsExportFormat.JSON));
			}
		}

		[Fact]
		private void HeaderTest()
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
					p["Col5"] = new TestClass() { ID = i, Name = (string)p["Col2"] };
					p.Update();
				}
				l.Debug($"headerTest source {i} rows added");
				l.Debug(p.ToString(TsExportFormat.JSON));
			}
		}
		[Fact]
		private void WriteDataTest()
		{
			using (FileLog l = new FileLog("DataPackageTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				int i = 5;
				DataPackage p = GetTestData(i, true, true);
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
		private void ToDataTableTest()
		{
			using (FileLog l = new FileLog("DataPackageTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				int i = 5;
				DataPackage p = GetTestData(i, false, true);
				l.Debug($"writeDataTest source {i} rows added");
				var t = p.ToDataTable();
				DisplayData(t, l);
			}
		}

		[Fact]
		private void SerializationSpeedTest()
		{
			using (FileLog l = new FileLog("DataPackageTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				l.Debug("SpeedTest MessagePackSerializer Start");
				int i = 0;
				for (i = 0; i < 1000000; i++)
				{
					TestClass test = new TestClass() { ID = i, Name = $"Item {i}" };
					test.ItemList.Add("Item {i}", i);
					var data = MessagePackSerializer.Typeless.Serialize(test);
					test = (TestClass)MessagePackSerializer.Typeless.Deserialize(data);
				}
				l.Debug("SpeedTest MessagePackSerializer Finish");
				l.Debug("SpeedTest JSONSerializer Start");
				for (i = 0; i < 1000000; i++)
				{
					TestClass test = new TestClass() { ID = i, Name = $"Item {i}" };
					test.ItemList.Add($"Item {i}", i);
					var data = JSONExtensions.SerializeObject(test);
					test = JSONExtensions.DeserializeObject<TestClass>(data);
				}
				l.Debug("SpeedTest JSOSerializer Finish");
			}
		}
		[MessagePackObject(keyAsPropertyName: true)]
		public class TestClass
		{
			public TestClass()
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
