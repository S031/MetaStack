using Xunit;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.Connectors;
using S031.MetaStack.Core;
using System.Collections.Generic;
using System;
using S031.MetaStack.Core.Data;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using S031.MetaStack.Core.Actions;
using S031.MetaStack.Core.Logging;

namespace MetaStack.Test.Services
{
	public class TCPConnectorTest
	{
		private readonly string  _cn;

		public TCPConnectorTest()
		{
			Program.ConfigureTests();
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
			var mdbTest = new MetaStack.Test.Data.MdbContextTest();
			_cn = mdbTest.connectionString;
		}

		[Fact]
		void SpeedTest4ConnectedSocket()
		{
			using (TCPConnector connector = TCPConnector.Create())
			{
				connector.Connect("Test", "@TestPassword");
			}
			//bag password
			using (TCPConnector connector = TCPConnector.Create())
			{
				//connector.Connect("Test", "@TestPasswor");
			}
		}

		[Fact]
		void ActionManagerTest()
		{
			using (FileLogger _logger = new FileLogger("ActionManagerTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			using (MdbContext mdb = new MdbContext(_cn))
			using (ActionManager am = new ActionManager(mdb))
			{
				am.Logger = _logger;
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
