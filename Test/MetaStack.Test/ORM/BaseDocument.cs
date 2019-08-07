using Newtonsoft.Json.Linq;
using S031.MetaStack.Core.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaStack.Test.ORM
{
	public class BaseDocument : JObject
	{
		private const string connection_string = "Data Source=localhost;Initial Catalog=Msfodb;Integrated Security=True";
		private static readonly Dictionary<string, JObject> _schemaCache = new Dictionary<string, JObject>();
		private readonly JObject _schema;
		private static readonly object obj4Lock = new object();

		public BaseDocument(string dbObjectName)
		{
			if (!_schemaCache.TryGetValue(dbObjectName, out _schema))
			{
				lock (obj4Lock)
				{
					_schemaCache.Add(dbObjectName, GetSchema(dbObjectName));
				}

				_schema = _schemaCache[dbObjectName];
			}
		}

		public int Save(bool IsNew)
		{
			string sql = new StatementWriter(_schema).WriteInsertStatement();
			return 0;
		}

		private static JObject GetSchema(string dbObjectName)
		{
			using (MdbContext mdb = new MdbContext(connection_string))
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
