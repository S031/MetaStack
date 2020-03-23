using S031.MetaStack.Common.Logging;
using System;
using Xunit;
using S031.MetaStack.Json;
using System.Text;

namespace MetaStack.Test.Json
{
	public class JSONExtensionsTest
	{
		private static readonly string _sourceJsonString = Encoding.UTF8.GetString(Resources.TestData.TestJson);
		private static readonly string _sourceJsonString2 = CommonTest.testData1.testJsonData;
		private static readonly byte[] _sourceJsonData = Resources.TestData.TestJsonData;
		private static readonly JsonValue _v = new JsonReader(_sourceJsonString).Read();

		public JSONExtensionsTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}


		[Fact]
		public void CastTest()
		{
			//using (FileLog _logger = new FileLog("JsonValueExtensionsTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			//{
			//	JValue v = new JValue("true");
			//	_logger.Write(LogLevels.Debug, "ToBoolOrDefault true", v.ToBoolOrDefault().ToString());
			//	v = new JValue(1);
			//	_logger.Write(LogLevels.Debug, "ToBoolOrDefault 1", v.ToBoolOrDefault().ToString());
			//	v = new JValue(100);
			//	_logger.Write(LogLevels.Debug, "ToBoolOrDefault 100", v.ToBoolOrDefault().ToString());
			//	v = new JValue(0);
			//	_logger.Write(LogLevels.Debug, "ToBoolOrDefault 0", v.ToBoolOrDefault().ToString());
			//	v = new JValue("false");
			//	_logger.Write(LogLevels.Debug, "ToBoolOrDefault false", v.ToBoolOrDefault().ToString());

			//	string source = @"{'string1':'value','integer2':99,'datetime3':'2000-05-23T00:00:00','time4':'22:00:00'}";
			//	JObject o = JObject.Parse(source);
			//	_logger.Write(LogLevels.Debug, "Source", source);
			//	_logger.Write(LogLevels.Debug, "ToBoolOrDefault string1", o["string1"].ToBoolOrDefault().ToString());
			//	_logger.Write(LogLevels.Debug, "ToBoolOrDefault integer2", o["integer2"].ToBoolOrDefault().ToString());
			//	_logger.Write(LogLevels.Debug, "ToBoolOrDefault datetime3", o["datetime3"].ToBoolOrDefault().ToString());
			//	_logger.Write(LogLevels.Debug, "ToBoolOrDefault time4", o["time4"].ToBoolOrDefault().ToString());
			//}
		}



		[Fact]
		public void ParseXML()
		{
			//using (FileLog _logger = new FileLog("JsonValueExtensionsTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			//{
			//	var v = JObject.Parse(_sourceJsonString2);
			//	_logger.Debug(v.ToString(Newtonsoft.Json.Formatting.Indented));
			//}
		}

		[Fact]
		public void JoinTest()
		{
			using (FileLog _logger = new FileLog("JsonValueExtensionsTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				JsonObject v = new JsonObject
				{
					["test1"] = 10,
					["test2"] = "test string",
					["text3"] = DateTime.Now
				};
				v.AddRange((JsonObject)_v);
				_logger.Debug(v.ToString(Formatting.Indented));
			}

		}

		[Fact]
		public void ToBoolOrDefaultTest()
		{
			//using (FileLog _logger = new FileLog("JsonValueExtensionsTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			//{
			//	string source = @"{'string1':'value','integer2':99,'datetime3':'2000-05-23T00:00:00','time4':'22:00:00'}";
			//	JObject o = JObject.Parse(source);
			//	_logger.Write(LogLevels.Debug, "Source", source);
			//	_logger.Write(LogLevels.Debug, "ToBoolOrDefault string1", o["string1"].ToBoolOrDefault().ToString());
			//	_logger.Write(LogLevels.Debug, "ToBoolOrDefault integer2", o["integer2"].ToBoolOrDefault().ToString());
			//	_logger.Write(LogLevels.Debug, "ToBoolOrDefault datetime3", o["datetime3"].ToBoolOrDefault().ToString());
			//	_logger.Write(LogLevels.Debug, "ToBoolOrDefault time4", o["time4"].ToBoolOrDefault().ToString());
			//}
		}

		[Fact]
		public void ToIntOrDefaultTest()
		{
			//using (FileLog _logger = new FileLog("JsonValueExtensionsTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			//{
			//	JObject _v = JObject.Parse(_sourceJsonString2);
			//	_logger.Debug(_v["agentId"].ToIntOrDefault());
			//	// string value
			//	_logger.Debug(_v["payComment"].ToIntOrDefault());
			//	// decimal value
			//	_logger.Debug(_v["svcNum"].ToIntOrDefault());
			//	_logger.Debug(((string)_v["svcNum"]).ToIntOrDefault());
			//	_logger.Debug("255255255".ToIntOrDefault());
			//}
		}

		[Fact]
		public void ToDecimalOrDefaultTest()
		{
			//using (FileLog _logger = new FileLog("JsonValueExtensionsTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			//{
			//	JObject v = JObject.Parse(_sourceJsonString2);
			//	decimal d = 987456123.97m;
			//	v["payAmount"] = d;
			//	_logger.Debug(v["payAmount"].ToDecimalOrDefault());
			//	_logger.Debug(v["svcNum"].ToDecimalOrDefault());
			//}
		}

		[Fact]
		public void ToDateOrDefaultTest()
		{
			//using (FileLog _logger = new FileLog("JsonValueExtensionsTests", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			//{
			//	JObject v = JObject.Parse(_sourceJsonString2);
			//	//if (v["dateCreate"].ToDateOrDefault().GetType() != typeof(DateTime))
			//	//	Assert.Fail();
			//	_logger.Debug(v["dateCreate"].ToDateOrDefault());
			//	_logger.Debug(v["svcNum"].ToDateOrDefault());
			//	_logger.Debug(v["svcNum"].ToDateOrDefault().IsEmpty());
			//}
		}
	}
}