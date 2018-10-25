using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S031.MetaStack.WinForms;
using S031.MetaStack.WinForms.Security;
using S031.MetaStack.WinForms.Connectors;
using S031.MetaStack.Common;
using System.Windows.Forms;
using System.Configuration;
using S031.MetaStack.WinForms.Data;

namespace S031.MetaStack.WinForms
{
	internal static class ClientGate
	{
		private static readonly string _appName = 
			System.Diagnostics.Process.GetCurrentProcess().ProcessName;

		private static TCPConnector _connector;

		public static TCPConnector Connector =>_connector;

		public static bool Logon()
		{
			if (_connector != null && _connector.Connected)
				return true;

			string userName = Environment.UserName;
			string password = string.Empty;
			bool savePassword = ConfigurationManager.AppSettings["SavePassword"].ToBoolOrDefault();
			bool isPrompt = false;
			var sInfo = CredentialManager.ReadCredential(_appName);
			if (sInfo == null || sInfo.Password.IsEmpty() || !savePassword)
			{
				password = SecureRequest(userName);
				if (password.IsEmpty())
					return false;
				isPrompt = true;
			}
			else
				password = sInfo.Password;
			try
			{
				_connector = TCPConnector.Create();
				_connector.Connect(userName, password);
				if (isPrompt && savePassword)
					CredentialManager.WriteCredential(_appName, userName, password);
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message, "Logon failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			return true;
		}

		public static void Logout() =>
			_connector?.Dispose();

		public static DataPackage Execute(string actionID, DataPackage paramTable)
		{
			Logon();
			return _connector.Execute(actionID, paramTable);
		}

		private static string SecureRequest(string userName)
		{
			return (string)InputBox.Show(new WinFormItem("Password")
			{
				Caption = "Введите пароль",
				PresentationType = typeof(TextBox),
				ControlTrigger = (i, c) =>
				{
					c.FindForm().Text = $"Вход в систему пользователя {userName}";
					TextBox tb = (c as TextBox);
					c.Width = 100;
					tb.PasswordChar = '*';
				}
			})[0];
		}

	}
}
