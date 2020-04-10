using System;
using S031.MetaStack.Common;
using System.Configuration;
using System.Data;
using S031.MetaStack.Data;
using S031.MetaStack.ORM;
using S031.MetaStack.Actions;
using S031.MetaStack.Interop.Connectors;
using S031.MetaStack.Security;

namespace S031.MetaStack.WinForms
{
	public static class ClientGate
	{
		private static readonly string _appName = 
			System.Diagnostics.Process.GetCurrentProcess().ProcessName;

		private static readonly MapTable<string, ActionInfo> _actionss =
			new MapTable<string, ActionInfo>();

		private static readonly MapTable<string, JMXSchema> _schemaCache
			= new MapTable<string, JMXSchema>();

		private static TCPConnector _connector;

		public static TCPConnector Connector =>_connector;

		public static bool Logon(ConnectorOptions connectorOptions)
		{
			if (_connector != null && _connector.Connected)
				return true;
			
			string userName = $@"{Environment.UserDomainName}\{Environment.UserName}";
			bool savePassword = connectorOptions.SavePassword;
			bool isPrompt = false;
			var sInfo = CredentialManager.ReadCredential(_appName);
			bool forcePassword = connectorOptions.ForcePassword || sInfo == null || sInfo.Password.IsEmpty() || !savePassword;
			string password;
			if (forcePassword)
			{
				password = connectorOptions.SecureRequest(userName);
				if (password.IsEmpty())
					return false;
				isPrompt = true;
			}
			else
				password = sInfo.Password;

			_connector = TCPConnector.Create(connectorOptions);
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

				_actionss.TryAdd(actionID, ai);
			}
			return ai;
		}

		public static JMXSchema GetObjectSchema(string objectName)
		{
			if (!_schemaCache.TryGetValue(objectName, out JMXSchema schema))
			{
				using (var p = new DataPackage(
						new string[] { "ObjectName" },
						new object[] { objectName }))
				using (var r = ClientGate.Execute("Sys.GetSchema", p))
				{
					r.GoDataTop();
					if (r.Read())
					{
						schema = JMXSchema.Parse((string)r["ObjectSchema"]);
						_schemaCache.TryAdd(objectName, schema);
					}
					else
						return null;
				}
			}
			return schema;
		}


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
					if (p.ToString().EndsWith("ConnectionName", StringComparison.OrdinalIgnoreCase))
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

	}
}
