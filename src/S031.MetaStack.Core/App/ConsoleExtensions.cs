using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace S031.MetaStack.Core.App
{
	public static class ConsoleExtensions
	{
		[DllImport("Kernel32")]
		private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

		static readonly bool _isWindows = isWindows();

		private delegate bool EventHandler(CtrlType sig);
		static EventHandler _handler;

		enum CtrlType
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT = 1,
			CTRL_CLOSE_EVENT = 2,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT = 6
		}

		static bool isWindows()
		{
			string windir = Environment.GetEnvironmentVariable("windir");
			return (!string.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir));
		}
		public static void OnExit(Func<bool> onExitCallBack)
		{
			if (_isWindows)
			{
				_handler += new EventHandler(ctrl => onExitCallBack());
				SetConsoleCtrlHandler(_handler, true);
			}
			else
			{
				// On future...
				System.AppDomain.CurrentDomain.ProcessExit += (c, e) =>
					{
						//onExitCallBack();
					};
			}
			//Ctrl+c pressed 
			Console.CancelKeyPress += (c, e) =>
				{
					e.Cancel = onExitCallBack();
				};
		}
	}
}
