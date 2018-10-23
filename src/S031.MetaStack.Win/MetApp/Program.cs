using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetApp
{
	static class Program
	{
		public const string AppName = "MetApp";
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			if (ClientGate.Logon())
			{
				AppDomain.CurrentDomain.ProcessExit += (s, e) => ClientGate.Logout();
				Application.Run(new Form1());
			}
		}
	}
}
