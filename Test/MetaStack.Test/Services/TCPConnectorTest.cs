using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.Actions;
using S031.MetaStack.Interop.Connectors;
using S031.MetaStack.Data;
using S031.MetaStack.Actions;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace MetaStack.Test.Services
{
	public class TCPConnectorTest
	{
		private readonly string _cn;
		private readonly ILogger _logger;

		public TCPConnectorTest()
		{
			_logger = Program
				.GetServices()
				.GetRequiredService<ILoggerProvider>()
				.CreateLogger("TCPConnectorTest");
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
			var mdbTest = new MetaStack.Test.Data.MdbContextTest();
			_cn = mdbTest.connectionString;
		}

		[Fact]
		private void TCPConnectorConnectTest()
		{
			using (FileLog l = new FileLog("TCPConnectorConnectTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			using (TCPConnector connector = TCPConnector.Create())
			{
				connector.Connect("Test", "@TestPassword");
				l.Debug("Start performance test for logins");
				int i = 0;
				for (i = 0; i < 1000; i++)
				{
					var dr = connector.Execute("Sys.Select", new DataPackage(new string[] { "ParamName", "ParamValue" },
						new object[] { "_connectionName", "banklocal" }));
					//string s = (string)dr["ObjectSchema"];
				}
				l.Debug($"End performance test for {i} logins");
			}
		}

		[Fact]
		private void ActionManagerTest()
		{
			var am = Program.GetServices().GetRequiredService<IActionManager>();
			ActionInfo ai = am.GetActionInfo("Sys.LoginRequest");
			_logger.LogDebug(ai.GetInputParamTable().ToString());
			_logger.LogDebug(ai.GetOutputParamTable().ToString());
			var dt = ai.GetOutputParamTable();
			dt.AddNew();
			dt["PublicKey"] = "123456789";
			dt.Update();
		}
	}
}
