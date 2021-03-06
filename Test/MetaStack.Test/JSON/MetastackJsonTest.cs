﻿using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.Actions;
using S031.MetaStack.Data;
using S031.MetaStack.Json;
using S031.MetaStack.Actions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;
using System.Text.Json;

namespace MetaStack.Test.Json
{
	public class MetaStackJsonTest
	{
		private static readonly string _sourceJsonString = Encoding.UTF8.GetString(Resources.TestData.TestJson);
		private static readonly byte[] _sourceJsonArrayData = Resources.TestData.TestJsonData;
		private static readonly byte[] _sourceJsonData = Resources.TestData.TestJson;
		private static readonly string _sourceUtf8JsonString = Encoding.UTF8.GetString(_sourceJsonArrayData);

		public MetaStackJsonTest()
		{
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}

		[Fact]
		public void PerformanceTest()
		{
			using (FileLog _logger = new FileLog(" MetaStackJson.PerformanceTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var str = _sourceJsonString;
				_logger.Debug($"Start perfomance parse string test");
				int i;
				DateTime t = DateTime.Now;
				for (i = 0; i < 10_000; i++)
				{
					var j = new JsonReader(str).Read();
				}
				_logger.Debug($"Finish perfomance parse string test. Time={(DateTime.Now - t).TotalMilliseconds} ms, loop count={i}");

				var options = new JsonDocumentOptions
				{
					AllowTrailingCommas = true
				}; 
				_logger.Debug($"Start perfomance JsonDocumet utf8 data parse test");
				t = DateTime.Now;
				var stream = new MemoryStream(_sourceJsonData, false);
				for (i = 0; i < 10_000; i++)
				{
					stream.Position = 0;
					_ = JsonDocument.Parse(new System.Buffers.ReadOnlySequence<byte>(_sourceJsonData), options);
				}
				_logger.Debug($"Finish perfomance JsonDocumet parse utf8 data test. Time={(DateTime.Now - t).TotalMilliseconds} ms, loop count={i}");
				
				_logger.Debug($"Start perfomance utf8 data parse test");
				t = DateTime.Now;
				stream = new MemoryStream(_sourceJsonData, false);
				for (i = 0; i < 10_000; i++)
				{
					stream.Position = 0;
					var j = new JsonReader(stream).Read();
				}
				_logger.Debug($"Finish perfomance parse utf8 data test. Time={(DateTime.Now - t).TotalMilliseconds} ms, loop count={i}");
				
				_logger.Debug($"Start perfomance utf8 string parse test");
				t = DateTime.Now;
				stream.Position = 0;
				for (i = 0; i < 10_000; i++)
				{
					var j = new JsonReader(Encoding.UTF8.GetString(stream.ToArray())).Read();
				}
				_logger.Debug($"Finish perfomance parse utf8 string test. Time={(DateTime.Now - t).TotalMilliseconds} ms, loop count={i}");

				_logger.Debug($"Start perfomance ToString test");
				var json = (JsonObject)new JsonReader(str).Read();
				//_logger.Debug(json.ToString());
				t = DateTime.Now;
				for (i = 0; i < 10_000; i++)
				{
					var s = json.ToString();
				}
				_logger.Debug($"Finish perfomance Tostring test. Time={(DateTime.Now - t).TotalMilliseconds} ms, loop count={i}");

				_logger.Debug($"Start perfomance GetIntOrDefault test");
				t = DateTime.Now;
				for (i = 0; i < 10_000_000; i++)
				{
					var s = (json as JsonObject).GetIntOrDefault("ID");
				}
				_logger.Debug($"Finish perfomance GetIntOrDefault test. Time={(DateTime.Now - t).TotalMilliseconds} ms, loop count={i}");

				_logger.Debug($"Start perfomance ToStringOrDefault test");
				t = DateTime.Now;
				for (i = 0; i < 10_000_000; i++)
				{
					var s = (json as JsonObject).GetStringOrDefault("NotPresent", "1234567890");
				}
				_logger.Debug($"Finish perfomance GetIntOrDefault test. Time={(DateTime.Now - t).TotalMilliseconds} ms, loop count={i}");

				_logger.Debug($"Start perfomance GetDateDefault test");
				t = DateTime.Now;
				json["CurrentTime"] = t;
				for (i = 0; i < 1_000_000; i++)
				{
					var s = (json as JsonObject).GetDateOrDefault("CurrentTime");
				}
				_logger.Debug($"Finish perfomance GetDateDefault test. Time={(DateTime.Now - t).TotalMilliseconds} ms, loop count={i}");
			}
		}

		[Fact]
		public void JsonUtf8ReaderTest()
		{
			using (FileLog _logger = new FileLog(" MetaStackJson.JsonUtf8ReaderTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var bs = Encoding.UTF8.GetBytes("{ \"a\": \"Тестовая строка\" }");
				var json = (JsonObject)new JsonReader(new MemoryStream(bs)).Read();
				_logger.Debug(json.ToString(Formatting.Indented));
			}
		}

		[Fact]
		public void JsonWriterTest()
		{
			using (FileLog _logger = new FileLog(" MetaStackJson.JsonWriterTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var str = _sourceJsonString;
				var json = (JsonObject)(new JsonReader(str).Read());
				json["EscapedString"] = str;
				var newStr = json.ToString(Formatting.Indented);
				json = (JsonObject)(new JsonReader(newStr).Read());
				Assert.Equal(json["EscapedString"], str);
				_logger.Debug(json.ToString(Formatting.Indented));
				str = _sourceUtf8JsonString;
				var jsonArray = (JsonArray)(new JsonReader(str).Read());
				_logger.Debug(jsonArray.ToString(Formatting.Indented));
			}
		}

		[Fact]
		public void JsonWriterObjectTest()
		{
			using (FileLog logger = new FileLog(" MetaStackJsoJsonWriterObjectTestn.", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				ActionInfo a = new ActionInfo
				{
					ActionID = "TestActionID",
					AssemblyID = "S031.MetaStack.Core.Actions",
					ClassName = "S031.MetaStack.Core.Actions.ActionInfo",
					Description = "public sealed class ActionInfo : InterfaceInfo",
					EMailOnError = true,
					IID = 1,
					InterfaceID = "IActionsTest",
					InterfaceName = "IActionsName",
					Name = "Test ActionInfo",
					TransactionSupport = TransactionActionSupport.Support,
					WebAuthentication = ActionWebAuthenticationType.Basic
				};
				for (int i = 0; i < 3; i++)
				{
					string name = $"Attrib{i}";
					ParamInfo p = new ParamInfo()
					{
						Agregate = $"COUNT({i})",
						AttribName = name,
						AttribPath = name,
						DataType = "int",
						DefaultValue = "0",
						Dirrect = ParamDirrect.Input,
						DisplayWidth = 10,
						FieldName = name,
						Name = $"Name of {name}",
						Position = i,
						Required = true
					};
					a.InterfaceParameters.Add(p);
				}
				var str = a.ToString();
				logger.Debug(str);
				a = ActionInfo.Parse(str);
				Assert.Equal(str, a.ToString());
				var json = (JsonObject)(new JsonReader(str).Read());
				logger.Debug((string)json["InterfaceParameters"][2]["Agregate"]);
			}
		}
		[Fact]
		public void JsonWriterWellKnownTest()
		{
			using (FileLog _logger = new FileLog("MetaStackJson.JsonWriterWellKnownTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				TestClass t = new TestClass() { ID = 1, Name = "MetaStackJson.JsonWriterWellKnownTest" };
				for (int i = 0; i < 10; i++)
					t.ItemList.Add($"key-{i}", $"Item value {i} for MetaStackJson.JsonWriterWellKnownTest");

				//write
				JsonWriter w = new JsonWriter(Formatting.Indented);
				w.WriteValue(new JsonValue(t));
				string str = w.ToString();
				_logger.Debug(str);

				//Read
				t = new TestClass();
				var r = new JsonReader(str).Read();
				t.ReadRaw(r);
				w = new JsonWriter(Formatting.Indented);
				w.WriteValue(new JsonValue(t));
				Assert.Equal(str, w.ToString());
			}
		}

		private class TestClass
		{
			static TestClass()
			{
				JsonWellKnownTypes.Register(
					new JsonAction(typeof(TestClass),
					(w, o) => (o as TestClass).WriteRaw(w),
					(r, o) => (o as TestClass).ReadRaw(r)));
			}

			public TestClass()
			{
			}

			public int ID { get; set; }

			public string Name { get; set; }

			public Dictionary<string, object> ItemList { get; set; }= new Dictionary<string, object>();

			public void WriteRaw(JsonWriter writer)
			{
				writer.WriteStartObject();
				writer.WriteProperty("ID", ID);
				writer.WriteProperty("Name", Name);
				if (ItemList.Count >0)
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
				ID = (int)o["ID"];
				Name = (string)o["Name"];
				JsonArray a = (JsonArray)o["ItemList"];
				foreach (var item in a)
				{
					var pair = (item as JsonObject).GetPair();
					ItemList.Add(pair.Key, pair.Value);
				}
			}
		}
	}
}