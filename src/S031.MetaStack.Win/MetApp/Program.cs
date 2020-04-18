using S031.MetaStack.Interop.Connectors;
using S031.MetaStack.Json;
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
			var config = new JsonReader(ConfigurationManager.AppSettings["TCPConnector"].Replace('\'', '"')).Read();
			ConnectorOptions options = new ConnectorOptions(config)
			{
				SecureRequest = SecureRequest.Show
			};
			try
			{
				ClientGate.Logon(options);
			}
			catch (Exception ex)
			{
				if (ex.GetType() == typeof(TCPConnectorException))
				{
					options.ForcePassword = true;
					ClientGate.Logon(options);
				}
				else
					throw; 
			}

		}
	}
}
