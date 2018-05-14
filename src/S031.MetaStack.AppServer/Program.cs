using S031.MetaStack.Core.App;
using System;
using System.Threading;

namespace S031.MetaStack.AppServer
{
	class Program
	{
		static void Main(string[] args)
		{
			using (var cts = new CancellationTokenSource())
			using (IAppHost host = new AppHost("config.json"))
			{
				Console.WriteLine($"{host.AppServerName} Starting...");
				ConsoleExtensions.OnExit(() => 
				{
					Console.WriteLine($"{host.AppServerName} Stoping...");
					cts.Cancel();
					return true;
				});
				Console.WriteLine($"{host.AppServerName} Started press Ctrl+C for stopping it");
				host.Run(cts.Token);
				cts.Token.WaitHandle.WaitOne();
				Console.WriteLine($"{host.AppServerName} Stoped");
			}
		}
	}
}