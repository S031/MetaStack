using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.Actions;
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
		private static readonly byte[] _sourceJsonData = Resources.TestData.TestJsonData;
		private static readonly string _sourceUtf8JsonString = Encoding.UTF8.GetString(_sourceJsonData);

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
				_logger.Debug($"Finish perfomance parse string test. Time={(DateTime.Now-t).Milliseconds} ms, loop count={i}");

				_logger.Debug($"Start perfomance ToString test");
				var json =(JsonObject) new JsonReader(ref str).Read();
				//_logger.Debug(json.ToString());
				t = DateTime.Now;
				for (i = 0; i < 10_000; i++)
				{
					var s = json.ToString();
				}
				_logger.Debug($"Finish perfomance Tostring test. Time={(DateTime.Now-t).Milliseconds} ms, loop count={i}");
			}
		}

		[Fact]
		public void JsonWriterTest()
		{
			using (FileLog _logger = new FileLog(" MetaStackJson.JsonWriterTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				var str = _sourceJsonString;
				var json = (JsonObject)(new JsonReader(ref str).Read());
				json["EscapedString"] = str;
				var newStr = json.ToString(Formatting.Indented);
				json = (JsonObject)(new JsonReader(ref newStr).Read());
				Assert.Equal(json["EscapedString"], str);
				_logger.Debug(json.ToString(Formatting.Indented));
				str = _sourceUtf8JsonString;
				var jsonArray = (JsonArray)(new JsonReader(ref str).Read());
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
				a = ActionInfo.Create(str);
				Assert.Equal(str, a.ToString());
				var json = (JsonObject)(new JsonReader(ref str).Read());
				logger.Debug((string)json["InterfaceParameters"][2]["Agregate"]);
			}
		}
	}
}