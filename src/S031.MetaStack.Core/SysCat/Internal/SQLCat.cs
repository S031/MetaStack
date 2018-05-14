using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Core.SysCat
{
	/// <summary>
	/// SQL Servber
	/// </summary>
	internal static class SQLCat
	{
		static readonly Dictionary<string, Dictionary<string, string>> _resources = new Dictionary<string, Dictionary<string, string>>(StringComparer.CurrentCultureIgnoreCase);
		public static Dictionary<string, string> GetCatalog(string providerName) => _resources[providerName];
		public static Dictionary<string, string> SetCatalog(string providerName, Dictionary<string, string> statements) => 
			_resources[providerName] = statements;

		/// <summary>
		/// !!! Реализолвать функциолнал RegisterPrivider(string provbiderName)
		/// </summary>
		static SQLCat()
		{
			SQLCatSQLServer.register();
			SQLCatSQLite.register();
		}
	}
}
