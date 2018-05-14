using Xunit;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.App;

namespace MetaStack.Test.App
{
	public class AppTest
	{
		public AppTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}
		[Fact]
		public void AppConfigTest()
		{
			using (FileLog l = new FileLog("AppConfigTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				AppConfig config = new AppConfig(System.IO.File.ReadAllText("config.json"));
				l.Debug("SpeedTest 1 Start ");
				int i = 0;
				for (i = 0; i < 1000000; i++)
				{
					var configItem = config.GetConfigItem("connectionStrings.BankLocal.ConnectionString");
				}
				l.Debug($"SpeedTest 1 Finish {i} count result {config.GetConfigItem("connectionStrings.bankLocal.connectionString")}");
				for (i = 0; i < 1000000; i++)
				{
					var configItem = config["connectionStrings"]["bankLocal"]["connectionString"];
				}
				l.Debug($"SpeedTest 2 Finish {i} count result {config["connectionStrings"]["bankLocal"]["connectionString"]}");
				for (i = 0; i < 1000000; i++)
				{
					var configItem = config["appSettings"];
				}
				l.Debug($"SpeedTest 2 Finish {i} count result {config["appSettings"]}");
			}
		}
	}
}
