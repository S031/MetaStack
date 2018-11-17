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
using S031.MetaStack.WinForms.Actions;
using S031.MetaStack.WinForms.ORM;
using System.Data;

namespace S031.MetaStack.WinForms
{
	public static class ClientGate
	{
		private static readonly object obj4Lock = new object();

		private static readonly string _appName = 
			System.Diagnostics.Process.GetCurrentProcess().ProcessName;

		private static readonly Dictionary<string, ActionInfo> _actionss =
			new Dictionary<string, ActionInfo>();

		private static TCPConnector _connector;

		public static TCPConnector Connector =>_connector;

		public static bool Logon()
		{
			if (_connector != null && _connector.Connected)
				return true;

			string userName = $@"{Environment.UserDomainName}\{Environment.UserName}";
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

			_connector = TCPConnector.Create();
			_connector.Connect(userName, password);
			if (isPrompt && savePassword)
				CredentialManager.WriteCredential(_appName, userName, password);
			return true;
		}

		public static void Logout() =>
			_connector?.Dispose();

		public static DataPackage Execute(string actionID, DataPackage paramTable = null)
		{
			Logon();
			try
			{
				return _connector.Execute(actionID, paramTable);
			}
			catch (System.IO.IOException)
			{
				//For reuse socket
				Logon();
				return _connector.Execute(actionID, paramTable);
			}
		}

		public static ActionInfo GetActionInfo(string actionID)
		{
			if (!_actionss.TryGetValue(actionID, out ActionInfo ai))
			{
				var dr = Execute("Sys.GetActionInfo",
					new DataPackage(new string[] { "ActionID" }, new object[] { actionID }));
				if (dr.Read())
					ai = ActionInfo.Create((string)dr["ActionInfo"]);
				else
					ai = null;

				lock (obj4Lock)
					if (!_actionss.ContainsKey(actionID))
						_actionss.Add(actionID, ai);
			}
			return _actionss[actionID];
		}

		public static JMXSchema GetObjectSchema(string objectName) => 
			JMXFactory
			.Create()
			.CreateJMXRepo()
			.GetSchema(objectName);

		public static DataTable GetData(string queryID, params object[] parameters)
		{
			var dr = ClientGate.GetActionInfo("Sys.Select")
			   .GetInputParamTable()
			   .AddNew()
			   .SetValue("ParamName", "_viewName")
			   .SetValue("ParamValue", queryID)
			   .Update();

			bool c = false;
			for (int i = 0; i < parameters.Length; i++)
			{
				object p = parameters[i];
				if (i % 2 == 0)
				{
					if (p.ToString().EndsWith("ConnectionName"))
						c = true;
					else
					{
						dr.AddNew();
						dr.SetValue("ParamName", (string)p);
					}
				}
				else
				{
					if (c)
					{
						dr.SetHeader("ConnectionName", p);
						dr.UpdateHeaders();
						c = false;
					}
					else
					{
						dr.SetValue("ParamValue", p);
						dr.Update();
					}
				}
			}
			return ClientGate.Execute("Sys.Select", dr).ToDataTable();
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
