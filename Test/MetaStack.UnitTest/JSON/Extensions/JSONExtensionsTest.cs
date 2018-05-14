using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.Json;
using Newtonsoft.Json.Linq;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace MetaStack.UnitTest
{
	[TestClass]
	public class JSONExtensionsTest
	{
		public JSONExtensionsTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}

		readonly JToken _v = JValue.Parse(CommonTest.testData1.testJsonData);
		readonly FileLog _logger = new FileLog("JsonValueExtensionsTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" });

		[TestMethod]
		public void castTest()
		{
			FileLog _logger = new FileLog("JsonValueExtensionsTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" });
			JValue v = new JValue("true");
			_logger.Write(LogLevels.Debug, "ToBoolOrDefault true", v.ToBoolOrDefault().ToString());
			v = new JValue(1);
			_logger.Write(LogLevels.Debug, "ToBoolOrDefault 1", v.ToBoolOrDefault().ToString());
			v = new JValue(100);
			_logger.Write(LogLevels.Debug, "ToBoolOrDefault 100", v.ToBoolOrDefault().ToString());
			v = new JValue(0);
			_logger.Write(LogLevels.Debug, "ToBoolOrDefault 0", v.ToBoolOrDefault().ToString());
			v = new JValue("false");
			_logger.Write(LogLevels.Debug, "ToBoolOrDefault false", v.ToBoolOrDefault().ToString());

			string source = @"{'string1':'value','integer2':99,'datetime3':'2000-05-23T00:00:00','time4':'22:00:00'}";
			JObject o = JObject.Parse(source);
			_logger.Write(LogLevels.Debug, "Source", source);
			_logger.Write(LogLevels.Debug, "ToBoolOrDefault string1", o["string1"].ToBoolOrDefault().ToString());
			_logger.Write(LogLevels.Debug, "ToBoolOrDefault integer2", o["integer2"].ToBoolOrDefault().ToString());
			_logger.Write(LogLevels.Debug, "ToBoolOrDefault datetime3", o["datetime3"].ToBoolOrDefault().ToString());
			_logger.Write(LogLevels.Debug, "ToBoolOrDefault time4", o["time4"].ToBoolOrDefault().ToString());
			_logger.Flush();
		}



		[TestMethod]
		public void ParseXML()
		{
			FileLog _logger = new FileLog("JsonValueExtensionsTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" });
			var v = JObject.Parse(CommonTest.testData1.testJsonData);
			_logger.Debug(v.ToString(Newtonsoft.Json.Formatting.Indented));
			_logger.Flush();
		}

		[TestMethod]
		public void JoinTest()
		{
			FileLog _logger = new FileLog("JsonValueExtensionsTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" });
			JObject v = new JObject
			{
				["test1"] = 10,
				["test2"] = "test string",
				["text3"] = DateTime.Now
			};
			v.Merge(_v);
			_logger.Debug(v.ToString(Newtonsoft.Json.Formatting.Indented));
			_logger.Flush();

		}

		[TestMethod]
		public void ToBoolOrDefaultTest()
		{
			FileLog _logger = new FileLog("JsonValueExtensionsTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" });
			_logger.Debug(_v["agentId"].ToBoolOrDefault());
			_logger.Debug(_v["isNormal"].ToBoolOrDefault());
			string source = @"{'string1':'value','integer2':99,'datetime3':'2000-05-23T00:00:00','time4':'22:00:00'}";
			JObject o = JObject.Parse(source);
			_logger.Write(LogLevels.Debug, "Source", source);
			_logger.Write(LogLevels.Debug, "ToBoolOrDefault string1", o["string1"].ToBoolOrDefault().ToString());
			_logger.Write(LogLevels.Debug, "ToBoolOrDefault integer2", o["integer2"].ToBoolOrDefault().ToString());
			_logger.Write(LogLevels.Debug, "ToBoolOrDefault datetime3", o["datetime3"].ToBoolOrDefault().ToString());
			_logger.Write(LogLevels.Debug, "ToBoolOrDefault time4", o["time4"].ToBoolOrDefault().ToString());
			_logger.Flush();
			_logger.Flush();
		}

		[TestMethod]
		public void ToIntOrDefaultTest()
		{
			FileLog _logger = new FileLog("JsonValueExtensionsTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" });
			_logger.Debug(_v["agentId"].ToIntOrDefault());
			// string value
			_logger.Debug(_v["payComment"].ToIntOrDefault());
			// decimal value
			_logger.Debug(_v["svcNum"].ToIntOrDefault());
			_logger.Debug(((string)_v["svcNum"]).ToIntOrDefault());
			_logger.Debug("255255255".ToIntOrDefault());
			_logger.Flush();
		}

		[TestMethod]
		public void ToDecimalOrDefaultTest()
		{
			FileLog _logger = new FileLog("JsonValueExtensionsTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" });
			JObject v = JObject.Parse(CommonTest.testData1.testJsonData);
			decimal d = 987456123.97m;
			v["payAmount"] = d;
			_logger.Debug(v["payAmount"].ToDecimalOrDefault());
			_logger.Debug(v["svcNum"].ToDecimalOrDefault());
			_logger.Flush();
		}

		[TestMethod]
		public void ToDateOrDefaultTest()
		{
			FileLog _logger = new FileLog("JsonValueExtensionsTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" });
			JObject v = JObject.Parse(CommonTest.testData1.testJsonData);
			//if (v["dateCreate"].ToDateOrDefault().GetType() != typeof(DateTime))
			//	Assert.Fail();
			_logger.Debug(_v["dateCreate"].ToDateOrDefault());
			_logger.Debug(_v["svcNum"].ToDateOrDefault());
			_logger.Debug(_v["svcNum"].ToDateOrDefault().IsEmpty());
			_logger.Flush();
		}
	}
}