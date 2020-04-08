using S031.MetaStack.Data;
using S031.MetaStack.ORM;
using System.Collections.Concurrent;

namespace S031.MetaStack.WinForms.ORM
{
	public class JMXClientRepo: JMXRepo
	{
		public override JMXSchema GetSchema(string objectName)
		{
			JMXSchema schema = ClientGate.GetObjectSchema(objectName);
			schema.SchemaRepo = this;
			return schema;
		}

		public override JMXSchema SaveSchema(JMXSchema schema)
			=> base.SaveSchema(schema);
	}
}
