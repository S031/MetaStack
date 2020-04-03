using S031.MetaStack.Data;
using S031.MetaStack.ORM;
using System.Collections.Concurrent;

namespace S031.MetaStack.WinForms.ORM
{
	public class JMXClientRepo: JMXRepo
	{
		private static readonly ConcurrentDictionary<string, JMXSchema> _schemaCache
			= new ConcurrentDictionary<string, JMXSchema>();
		public override JMXSchema GetSchema(string objectName)
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
						schema.SchemaRepo = this;
						_schemaCache.TryAdd(objectName, schema);
					}
					else
						return null;
				}
			}
			return schema;
		}
		public override JMXSchema SaveSchema(JMXSchema schema)
		{
			return base.SaveSchema(schema);
		}
	}
}
