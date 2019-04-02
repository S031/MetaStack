using Newtonsoft.Json.Linq;
using S031.MetaStack.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaStack.Test.ORM
{
	public class BaseDocument : JObject
	{
		static readonly Dictionary<string, JObject> _schemaCache = new Dictionary<string, JObject>();
		private readonly JObject _schema;
		private static object obj4Lock = new object();

		public BaseDocument(string dbObjectName)
		{
			if (!_schemaCache.TryGetValue(dbObjectName, out _schema))
			{
				lock (obj4Lock)
					_schemaCache.Add(dbObjectName, GetSchema(dbObjectName));
				_schema = _schemaCache[dbObjectName];
			}
		}

		private static JObject GetSchema(string dbObjectName)
		{
			using (MdbContext mdb = new MdbContext(""))
			{
				string s = mdb.Execute<string>(@"[HQ\SVOSTRIKOV].Get_TableSchema",
					new MdbParameter("@table_name", dbObjectName));
				return JObject.Parse(s);
			}
		}


	}

	internal class StatementWriter
	{
		private readonly JObject _schema;

		public StatementWriter(JObject schema)
		{
			_schema = schema;
		}

		public string WriteInsertStatement()
		{
			var dbName = _schema["DBObjectName"];
			JArray attribs = (JArray)_schema["Attributes"];
			StringBuilder sb = new StringBuilder($"INSERT INTO [{dbName["AreaName"]}].[{dbName["ObjectName"]}] (");
			sb.Append(string.Join(",", attribs.Where(a => !((JObject)a).Properties().Any(p => p.Name == "Identity")).Select(a => (string)a["FieldName"])));
			sb.Append(") VALUES (");
			sb.Append(string.Join(",", attribs.Where(a => !((JObject)a).Properties().Any(p => p.Name == "Identity")).Select(a => "@" + (string)a["FieldName"])));

			return sb.ToString();
		}

	}
}
