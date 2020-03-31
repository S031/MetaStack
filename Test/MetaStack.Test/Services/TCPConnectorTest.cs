﻿using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.Actions;
using S031.MetaStack.Core.Connectors;
using S031.MetaStack.Core.Logging;
using S031.MetaStack.Data;
using Xunit;

namespace MetaStack.Test.Services
{
	public class TCPConnectorTest
	{
		private readonly string _cn;

		public TCPConnectorTest()
		{
			Program.ConfigureTests();
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
			var mdbTest = new MetaStack.Test.Data.MdbContextTest();
			_cn = mdbTest.connectionString;
		}

		[Fact]
		private void TCPConnectorConnectTest()
		{
			using (FileLogger l = new FileLogger("TCPConnectorConnectTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
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
			using (FileLogger _logger = new FileLogger("ActionManagerTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			using (MdbContext mdb = new MdbContext(_cn))
			using (ActionManager am = new ActionManager(mdb) { Logger = _logger })
			{
				ActionInfo ai = am.GetActionInfo("Sys.LoginRequest");
				_logger.Debug(ai.GetInputParamTable().ToString());
				_logger.Debug(ai.GetOutputParamTable().ToString());
				var dt = ai.GetOutputParamTable();
				dt.AddNew();
				dt["PublicKey"] = "123456789";
				dt.Update();
			}
		}
	}
}
