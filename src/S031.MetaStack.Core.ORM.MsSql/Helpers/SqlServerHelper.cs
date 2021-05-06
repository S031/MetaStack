using S031.MetaStack.Common;
using S031.MetaStack.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Core.ORM.MsSql
{
	internal static class SqlServerHelper
	{
		const int _sql_server_2017_version = 14;
		public static readonly JMXSQLTypeMapping TypeMapping = new JMXSQLTypeMapping();
		public static IReadOnlyDictionary<MdbType, string> TypeMap => TypeMapping.GetTypeMap();
		public static IReadOnlyDictionary<string, MdbTypeInfo> TypeInfo => TypeMapping.GetServerTypeMap();
		private static readonly MapTable<int, string> _versions = new MapTable<int, string>();

		public static async Task<bool> IsSql17(MdbContext mdb)
			=> (await GetSqlVersion(mdb)).ToIntOrDefault() >= _sql_server_2017_version;

		private static async Task<string> GetSqlVersion(MdbContext mdb)
		{
			int mdbHash = mdb.MdbHash();
			if (!_versions.TryGetValue(mdbHash, out string sqlVersion))
			{
				sqlVersion = await mdb.ExecuteAsync<string>(SqlServer.SQLVersion);
				if (sqlVersion.IsEmpty())
					sqlVersion = "10";
				else
					sqlVersion = sqlVersion.GetToken(0, ".");
				_versions.Add(mdbHash, sqlVersion);
			}
			return sqlVersion;
		}

		private static int MdbHash(this MdbContext mdb)
			=> mdb.ConnectInfo.ConnectionString.GetHashCode();
	}
}
