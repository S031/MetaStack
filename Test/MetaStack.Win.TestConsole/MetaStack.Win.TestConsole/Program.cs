using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
			//var connectionName = Dns.GetHostName() == "SERGEY-WRK" ? "Test" : "BankLocal";
			//var connectionName = "SqliteDb";
			using (FileLog l = new FileLog("TCPConnectorConnectTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				ClientGate.Logon();
				DateTime t = DateTime.Now;
				Console.WriteLine("Start performance test for Sys.Select");
				//int i = 0;
				//for (int i = 0; i < 10000; i++)
				Parallel.For(0, 50000, (i, c) =>
				{
					var dr = ClientGate.GetActionInfo("A_TestForFct")
					   .GetInputParamTable()
					   .AddNew()
					   .SetValue("@ObjectName", "SysCat.SysSchemas")
					   .SetValue("@IDs", "1")
					   .Update();
					ClientGate.Execute("A_TestForFct", dr);
					if (i % 1000 == 0)
					{
						//dr.Read();
						Console.WriteLine($"{i}\t{dr[0]}{DateTime.Now.Second}");
					}
					//try
					//{
					//	var dr = ClientGate.GetData("dbo.V_DealValue",
					//		"@Date", new DateTime(2018, 10, 4),
					//		"@BranchID", 2000,
					//		"@DealType", "");
					//	if (i % 1000 == 0)
					//	{
					//		//dr.Read();
					//		Console.WriteLine($"{i}\t{dr.Rows[0][0]}{DateTime.Now.Second}");
					//	}
					//}
					//catch (Exception ex)
					//{
					//	Console.WriteLine($"{i}\t{ex.Message}");
					//	c.Break();
					//}

				});
				Console.WriteLine($"End performance test for {(DateTime.Now - t).TotalMilliseconds} ms Sys.Select");
				ClientGate.Logout();

				//l.Debug("Start performance test for logins");
				//for (i = 0; i < 10000; i++)
				//{
				//	DataPackage dr = new DataPackage("ObjectName.String.64");
				//	dr.SetHeader("ConnectionName", "Test");
				//	dr.AddNew();
				//	dr["ObjectName"] = "SysCat.SysSchema";
				//	dr.Update();

				//	dr = ClientGate.Execute("Sys.GetSchema", dr);
				//	if (i % 1000 == 0)
				//	{
				//		dr.Read();
				//		Console.WriteLine($"{i}\t{DateTime.Now.Second}");
				//		//Console.WriteLine(i);
				//	}
				//}
				//l.Debug($"End performance test for {i} logins");
			}
		}
	}
}
