using S031.MetaStack.WinForms.Data;

namespace S031.MetaStack.WinForms.ORM
{
	public class JMXClientRepo: JMXRepo
	{
		public override JMXSchema GetSchema(string objectName)
		{
			using (var p = new DataPackage(
					new string[] { "ObjectName" },
					new object[] { objectName }))
			using (var r = ClientGate.Execute("Sys.GetSchema", p))
			{
				r.GoDataTop();
				if (r.Read())
				{
					JMXSchema schema = JMXSchema.Parse((string)r["ObjectSchema"]);
					schema.SchemaRepo = this;
					return schema;
				}
			}
			return null;
		}
		public override JMXSchema SaveSchema(JMXSchema schema)
		{
			return base.SaveSchema(schema);
		}
	}
}
