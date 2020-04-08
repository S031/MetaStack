﻿using S031.MetaStack.Interop.Connectors;
using S031.MetaStack.WinForms;
using System;
using System.Configuration;
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

			try
			{
				Logon();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"{ex.Message}\n{ex.StackTrace}", "Logon failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			AppDomain.CurrentDomain.ProcessExit += (s, e) => ClientGate.Logout();
			Application.Run(new MainForm(ConfigurationManager.AppSettings["StartupForm"]));
		}

		private static void Logon()
		{
			try
			{
				ClientGate.Logon(false, SecureRequest.Show);
			}
			catch (Exception ex)
			{
				if (ex.GetType() == typeof(TCPConnectorException))
					ClientGate.Logon(true, SecureRequest.Show);
				else
					throw; 
			}

		}
	}
}
