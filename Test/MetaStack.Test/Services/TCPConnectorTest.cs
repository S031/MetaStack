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

namespace MetaStack.Test.Services
{
	public class TCPConnectorTest
	{
		public TCPConnectorTest()
		{
			Program.ConfigureTests();
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}

		[Fact]
		void SpeedTest4ConnectedSocket()
		{
			using (TCPConnector connector = TCPConnector.Create())
			{

			}
		}
	}
}
