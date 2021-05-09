using S031.MetaStack.Common;
using S031.MetaStack.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Core.ORM.SQLite
{
	internal static class SQLiteHelper
	{
		public static readonly JMXSQLiteTypeMapping TypeMapping = new JMXSQLiteTypeMapping();
		public static IReadOnlyDictionary<MdbType, string> TypeMap => TypeMapping.GetTypeMap();
		public static IReadOnlyDictionary<string, MdbTypeInfo> TypeInfo => TypeMapping.GetServerTypeMap();
		private static string _sqlVersion = string.Empty;
		public static async Task<string> GetSqlVersion(MdbContext mdb)
		{
			if (_sqlVersion.IsEmpty())
			{
				_sqlVersion = await mdb.ExecuteAsync<string>(SQLite.SQLVersion);
				if (_sqlVersion.IsEmpty())
					_sqlVersion = "3";
				else
					_sqlVersion = _sqlVersion.GetToken(0, ".");
			}
			return _sqlVersion;
		}
	}
}
