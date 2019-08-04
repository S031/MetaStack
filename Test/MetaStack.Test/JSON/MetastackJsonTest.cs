using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Json;
using System;
using System.Globalization;
using System.Text;
using Xunit;


namespace MetaStack.Test.Json
{
	public class MetaStackJsonTest
	{
		private static readonly string _sourceJsonString = Encoding.UTF8.GetString(Resources.TestData.TestJson);
		byte[] _sourceJsonData = Resources.TestData.TestJsonData;

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
					var j = new JsonReader(ref str).Read();
				}
				_logger.Debug($"Finish perfomance parse string test. Time={(DateTime.Now-t).Milliseconds}ms, loop count={i}");

				_logger.Debug($"Start perfomance ToString test");
				var json =(JsonObject) new JsonReader(ref str).Read();
				//_logger.Debug(json.ToString());
				t = DateTime.Now;
				for (i = 0; i < 10_000; i++)
				{
					var s = json.ToString();
				}
				_logger.Debug($"Finish perfomance Tostring test. Time={(DateTime.Now-t).Milliseconds}ms, loop count={i}");
			}
		}

		[Fact]
		public void JsonWriterTest()
		{
			using (FileLog _logger = new FileLog(" MetaStackJson.JsonWriterTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var str = _sourceJsonString;
				var json = (JsonObject)(new JsonReader(ref str).Read());
				_logger.Debug(json.ToString(Formatting.None));
			}
		}
	}
}