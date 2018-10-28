using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.WinForms;
using S031.MetaStack.WinForms.Connectors;
using S031.MetaStack.WinForms;
using S031.MetaStack.WinForms.Data;

namespace MetaStack.Win.TestConsole
{
	class Program
	{
		static void Main(string[] args)
		{
			//var connectionName = Dns.GetHostName() == "SERGEY-WRK" ? "Test" : "BankLocal";
			//var connectionName = "SqliteDb";
			using (FileLog l = new FileLog("TCPConnectorConnectTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				l.Debug("Start performance test for Sys.Select");
				int i = 0;
				for (i = 0; i < 10000; i++)
				{
					var dr = ClientGate.Execute("Sys.Select",
					   ClientGate.GetActionInfo("Sys.Select")
					   .GetInputParamTable()
					   .SetHeader("ConnectionName", "Test")
					   .UpdateHeaders()
					   .AddNew()
					   .SetValue("ParamName", "_viewName")
					   .SetValue("ParamValue", "SysCat.SysSchema")
					   .Update()
					   .AddNew()
					   .SetValue("ParamName", "_filter")
					   .SetValue("ParamValue", "ObjectName = 'SysSchema'")
					   .Update()
					   );

					if (i % 1000 == 0)
					{
						dr.Read();
						Console.WriteLine($"{i}\t{dr["ObjectName"]}{DateTime.Now.Second}");
					}
				}
				l.Debug($"End performance test for {i} Sys.Select");

				l.Debug("Start performance test for logins");
				for (i = 0; i < 10000; i++)
				{
					DataPackage dr = new DataPackage("ObjectName.String.64");
					dr.SetHeader("ConnectionName", "Test");
					dr.AddNew();
					dr["ObjectName"] = "SysCat.SysSchema";
					dr.Update();

					dr = ClientGate.Execute("Sys.GetSchema", dr);
					if (i % 1000 == 0)
					{
						dr.Read();
						Console.WriteLine($"{i}\t{DateTime.Now.Second}");
						//Console.WriteLine(i);
					}
				}
				l.Debug($"End performance test for {i} logins");
			}
		}
	}
}
