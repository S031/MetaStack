using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.WinForms;
using S031.MetaStack.WinForms.Connectors;
using S031.MetaStack.WinForms.Data;

namespace MetaStack.Win.TestConsole
{
	class Program
	{
		static void Main(string[] args)
		{
			using (FileLog l = new FileLog("TCPConnectorConnectTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			using (TCPConnector connector = TCPConnector.Create())
			{
				connector.Connect("Test", "@TestPassword");
				l.Debug("Start performance test for logins");
				int i = 0;
				for (i = 0; i < 10000; i++)
				{
					connector.Execute("Sys.Select", new DataPackage(new string[] { "ParamName", "ParamValue" }, new object[] { "_connectionName", "Test" }));
					if (i % 1000 == 0)
						Console.WriteLine(i);
				}
				l.Debug($"End performance test for {i} logins");
			}
		}
	}
}
