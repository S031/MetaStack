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
	}
}
