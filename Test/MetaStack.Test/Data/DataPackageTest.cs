﻿using S031.MetaStack.Common.Logging;
using S031.MetaStack.Data;
using S031.MetaStack.Json;
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
					p["Col5"] = null;
					p.Update();
				}
				p.GoDataTop();
				for (; p.Read();)
				{
					l.Debug(p.GetRowJSON());
				}
				l.Debug(p.ToString(TsExportFormat.JSON));

				p = new DataPackage("Col1", 999999999999, "Col2", "Свойство 1", "Col3", DateTime.Now, "Col4", Guid.NewGuid());
				p.SetHeader("HeaderValue1", 100)
					.SetHeader("HeaderValue2", DateTime.Now)
					.UpdateHeaders();
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
				int count = 1_000;
				int loopCount = 1_000;
				DataPackage p = GetTestData(count, true, false);
				l.Debug("SpeedTest Start");
				for (i = 0; i < loopCount; i++)
					p = GetTestData(count, true, false);
				l.Debug($"SpeedTest Finish {i} packages with {count} rows");
				for (i = 0; i < loopCount; i++)
					p.GoDataTop();
					for (; p.Read();)
					{
					}
				l.Debug($"SpeedTest Finish {count*loopCount} rows readed");
				byte[] data =  p.ToArray();
				for (i = 0; i < loopCount; i++)
					_ = p.ToArray();
				l.Debug($"SpeedTest Finish {i} serialize with {count} rows");
				for (i = 0; i < loopCount; i++)
					p = new DataPackage(data);
				l.Debug($"SpeedTest Finish {i} deserialize with {count} rows");
			}
		}

		private static readonly DateTime _dateTest = DateTime.Now.Date;
		private static DataPackage GetTestData(int rowsCount = 5, bool withHeader = false, bool withObjectData = false)
		{
			DataPackage p = new DataPackage(125, new string[] { "Col1.int", "Col2.string.255", "Col3.datetime.10", "Col4.Guid.34", "Col5.object.10.true" }, null);
			for (int i = 0; i < rowsCount; i++)
			{
				p.AddNew()
					.SetValue(0, i)
					.SetValue(1, $"Строка # {new string('x', i % 100)}")
					.SetValue(2, DateTime.Now.AddDays(i))
					.SetValue(3, Guid.NewGuid());

				if (!withObjectData)
					p[4] = null; //_dateTest (+4 sec for 10_000_000 loops);
				else
					p[4] = new TestClass() { ID = i, Name = (string)p[1] };
				p.Update();
			}
			//Specialy update headers after input data for test speed shifting 1000 loop for shift 1000 rows ~ 2.5c
			//update headers before input data ~ 1.7c
			if (withHeader)
				p.SetHeader("Username", "Сергей")
				.SetHeader("Password", "1234567T")
				.SetHeader("Sign", UnicodeEncoding.UTF8.GetBytes("Сергей"))
				.UpdateHeaders();
			return p;
		}

		[Fact]
		private void ByteArrayTest()
		{
			using (FileLog l = new FileLog("DataPackageTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				int i = 5;
				DataPackage p = GetTestData(i, true, false);
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
				string json = p.ToString(TsExportFormat.JSON);
				l.Debug(json);
				JsonObject j = JsonObject.Parse(json);
				int headerSize = (int)j["HeaderSize"];
				int hash = json.GetHashCode();
				p = DataPackage.Parse(p.ToString(TsExportFormat.JSON), TsJsonFormat.Full, headerSize);
				l.Debug("parseTest after conversion");
				l.Debug(p.ToString(TsExportFormat.JSON));
				Assert.True(hash == p.ToString(TsExportFormat.JSON).GetHashCode());

				string source = "{\"string1\":\"value\",\"integer2\":99,\"datetime3\":\"2017-05-23T00:00:00\",\"time4\":\"22:00:00\"}";
				p = DataPackage.Parse(source, TsJsonFormat.Simple, 8);
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
					p["Col5"] = null;
					//p["Col5"] = new TestClass() { ID = i, Name = (string)p["Col2"] };
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
				DataPackage p = GetTestData(i, true, false);
				l.Debug($"writeDataTest source {i} rows added");
				l.Debug(p.ToString(TsExportFormat.JSON));
				int hash = p.ToString(TsExportFormat.JSON).GetHashCode();
				var p1 = new DataPackage(p);
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
				DataPackage p = GetTestData(i, false, false);
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
				//l.Debug("SpeedTest MessagePackSerializer custom object Start");
				//int i = 0;
				//for (i = 0; i < 1_000_000; i++)
				//{
				//	TestClass test = new TestClass() { ID = i, Name = $"Item {i}" };
				//	test.ItemList.Add("Item {i}", i);
				//	var data = MessagePackSerializer.Typeless.Serialize(test);
				//	test = (TestClass)MessagePackSerializer.Typeless.Deserialize(data);
				//}
				//l.Debug("SpeedTest MessagePackSerializer custom object Finish");

				//l.Debug("SpeedTest DateTime object Start");
				//object d = DateTime.Now;
				//for (i = 0; i < 1_000_000; i++)
				//{
				//	var data = MessagePackSerializer.Typeless.Serialize(d);
				//	d = MessagePack.MessagePackSerializer.Typeless.Deserialize(data);
				//}
				//l.Debug("SpeedTest JSOSerializer Finish");

				l.Debug("SpeedTest JSONSerializer Start");
				string data = "";
				for (int i = 0; i < 1_000_000; i++)
				{
					TestClass test = new TestClass() { ID = i, Name = $"Item {i}" };
					test.ItemList.Add($"Item {i}", i);
					test.ItemList.Add($"Item", "Просто");
					data = JsonSerializer.SerializeObject(test);
					_ = JsonSerializer.DeserializeObject<TestClass>(data);
				}
				l.Debug(data);
				l.Debug("SpeedTest JSOSerializer Finish");
			}
		}

		public class TestClass
		{
			static TestClass()
			{
				//JsonWellKnownTypes.Register(
				//	new JsonAction(typeof(TestClass),
				//	(w, o) => (o as TestClass).WriteRaw(w),
				//	(r, o) => (o as TestClass).ReadRaw(r)));
			}

			public TestClass()
			{
			}

			public int ID { get; set; }

			public string Name { get; set; }

			public Dictionary<string, object> ItemList { get; set; } = new Dictionary<string, object>();

			public void WriteRaw(JsonWriter writer)
			{
				writer.WriteStartObject();
				writer.WriteProperty("ID", ID);
				writer.WriteProperty("Name", Name);
				if (ItemList.Count > 0)
				{
					writer.WritePropertyName("ItemList");
					writer.WriteStartArray();
					foreach (var item in ItemList)
					{
						writer.WriteStartObject();
						writer.WriteProperty(item.Key, item.Value);
						writer.WriteEndObject();
					}
					writer.WriteEndArray();
				}
				writer.WriteEndObject();
			}

			public void ReadRaw(JsonValue value)
			{
				JsonObject o = value as JsonObject;
				ID = o.GetIntOrDefault("ID");
				Name = o.GetStringOrDefault("Name");
				JsonArray a = (JsonArray)o["ItemList"];
				foreach (var item in a)
				{
					var pair = (item as JsonObject).GetPair();
					ItemList.Add(pair.Key, pair.Value);
				}
			}
		}

		private static void DisplayData(System.Data.DataTable table, FileLog l)
		{
			StringBuilder sb = new StringBuilder();
			foreach (System.Data.DataRow row in table.Rows)
			{
				foreach (System.Data.DataColumn col in table.Columns)
				{
					sb.AppendFormat("{0} = {1}\t", col.ColumnName, DBNull.Value.Equals(row[col]) ? "Null" : row[col]);
				}
				sb.AppendLine();
			}
			l.Debug(sb.ToString());
		}
	}
}
