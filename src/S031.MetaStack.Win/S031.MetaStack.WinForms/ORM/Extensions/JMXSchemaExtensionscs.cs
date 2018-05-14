using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.WinForms.ORM
{
	public static class JMXSchemaExtensionscs
	{
		public static IEnumerable<string> GetChildObjects(this JMXSchema  schema)
		{
			if (schema.SchemaProvider == null)
				return JMXSchemaProviderFactory.Default.GetChildObjects(schema.ObjectName);
			else
				return schema.SchemaProvider.GetChildObjects(schema.ObjectName);
		}
	}
}
