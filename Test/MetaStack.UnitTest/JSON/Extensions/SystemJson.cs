using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;
using System;
using System.Json;
using Xunit;


namespace CommonTest
{
	public class SystemJsonTest
	{
		public SystemJsonTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}

		JsonObject _v = (JsonObject)JsonObject.Parse(CommonTest.testData.testJsonData);
		FileLog _logger = new FileLog("SystemJsonTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" });

		[Fact]
		void castTest()
		{
			using (FileLog l = new FileLog("SystemJsonTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var v = new JsonPrimitive("true");
				l.Write(LogLevels.Debug, "ToBoolOrDefault true", v.ToBoolOrDefault().ToString());
				v = new JsonPrimitive(1);
				l.Write(LogLevels.Debug, "ToBoolOrDefault 1", v.ToBoolOrDefault().ToString());
				v = new JsonPrimitive(100);
				l.Write(LogLevels.Debug, "ToBoolOrDefault 100", v.ToBoolOrDefault().ToString());
				v = new JsonPrimitive(0);
				l.Write(LogLevels.Debug, "ToBoolOrDefault 0", v.ToBoolOrDefault().ToString());
				v = new JsonPrimitive("false");
				l.Write(LogLevels.Debug, "ToBoolOrDefault false", v.ToBoolOrDefault().ToString());

				string source = @"{""string1"":""value"",""integer2"":99,""datetime3"":""2000-05-23T00:00:00"",""time4"":""22:00:00""}";
				JsonObject o = (JsonObject)JsonObject.Parse(source);
				l.Write(LogLevels.Debug, "Source", source);
				l.Write(LogLevels.Debug, "ToBoolOrDefault string1", o["string1"].ToBoolOrDefault().ToString());
				l.Write(LogLevels.Debug, "ToBoolOrDefault integer2", o["integer2"].ToBoolOrDefault().ToString());
				l.Write(LogLevels.Debug, "ToBoolOrDefault datetime3", o["datetime3"].ToBoolOrDefault().ToString());
				l.Write(LogLevels.Debug, "ToBoolOrDefault time4", o["time4"].ToBoolOrDefault().ToString());
			}
		}



		[Fact]
		public void ParseXML()
		{
			using (FileLog l = new FileLog("SystemJsonTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var v = JsonObject.Parse(CommonTest.testData.testJsonData);
				l.Debug(v.ToStringFormatted(IndentCharValue.INDENT_TAB));
			}
		}

		[Fact]
		public void JoinTest()
		{
			using (FileLog l = new FileLog("SystemJsonTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				JsonObject v = new JsonObject();
				v["test1"] = 10;
				v["test2"] = "test string";
				v["text3"] = DateTime.Now;
				v.Join(_v);
				l.Debug(v.ToStringFormatted(IndentCharValue.INDENT_TAB));
			}

		}

		[Fact]
		public void ToBoolOrDefaultTest()
		{
			using (FileLog l = new FileLog("SystemJsonTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				_logger.Debug(_v["agentId"].ToBoolOrDefault());
				_logger.Debug(_v["isNormal"].ToBoolOrDefault());
				string source = "{\"string1\":\"value\",\"integer2\":99,\"datetime3\":\"2000-05-23T00:00:00\",\"time4\":\"22:00:00\"}";
				JsonObject o = (JsonObject)JsonObject.Parse(source);
				l.Write(LogLevels.Debug, "Source", source);
				l.Write(LogLevels.Debug, "ToBoolOrDefault string1", o["string1"].ToBoolOrDefault().ToString());
				l.Write(LogLevels.Debug, "ToBoolOrDefault integer2", o["integer2"].ToBoolOrDefault().ToString());
				l.Write(LogLevels.Debug, "ToBoolOrDefault datetime3", o["datetime3"].ToBoolOrDefault().ToString());
				l.Write(LogLevels.Debug, "ToBoolOrDefault time4", o["time4"].ToBoolOrDefault().ToString());
				l.Flush();
			}
		}

		[Fact]
		public void ToIntOrDefaultTest()
		{
			using (FileLog l = new FileLog("SystemJsonTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				l.Debug(_v["agentId"].ToIntOrDefault());
				// string value
				l.Debug(_v["payComment"].ToIntOrDefault());
				// decimal value
				l.Debug(_v["svcNum"].ToIntOrDefault());
				l.Debug(((string)_v["svcNum"]).ToIntOrDefault());
				l.Debug("255255255".ToIntOrDefault());
			}
		}

		[Fact]
		public void ToDecimalOrDefaultTest()
		{
			using (FileLog l = new FileLog("SystemJsonTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				JsonObject v = (JsonObject)JsonObject.Parse(CommonTest.testData.testJsonData);
				decimal d = 987456123.97m;
				v["payAmount"] = d;
				l.Debug(v["payAmount"].ToDecimalOrDefault());
				l.Debug(v["svcNum"].ToDecimalOrDefault());
			}
		}

		[Fact]
		public void ToDateOrDefaultTest()
		{
			using (FileLog l = new FileLog("SystemJsonTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				JsonObject v = (JsonObject)JsonObject.Parse(CommonTest.testData.testJsonData);
				l.Debug(_v["dateCreate"].ToDateOrDefault());
				l.Debug(_v["svcNum"].ToDateOrDefault());
				l.Debug(_v["svcNum"].ToDateOrDefault().IsEmpty());
			}
		}
	}
}