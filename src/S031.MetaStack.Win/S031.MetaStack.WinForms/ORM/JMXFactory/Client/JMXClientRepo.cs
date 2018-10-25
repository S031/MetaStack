using S031.MetaStack.WinForms.Data;

namespace S031.MetaStack.WinForms.ORM
{
	public class JMXClientRepo: JMXRepo
	{
		public override JMXSchema GetSchema(string objectName)
		{
			var r = ClientGate.Execute("Sys.GetSchema",
				new DataPackage(new string[] { "ObjectName" },
					new object[] { objectName }));
			r.GoDataTop();
			if (r.Read())
			{
				JMXSchema schema = JMXSchema.Parse((string)r["ObjectSchema"]);
				schema.SchemaRepo = this;
				return schema;
			}
			return null;
		}
		public override JMXSchema SaveSchema(JMXSchema schema)
		{
			return base.SaveSchema(schema);
		}
	}
}
