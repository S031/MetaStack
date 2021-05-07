using S031.MetaStack.Data;
using S031.MetaStack.ORM;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace S031.MetaStack.WinForms.ORM
{
	public class JMXClientRepo: JMXRepo
	{
		public override void ClearCatalog() => throw new System.NotImplementedException();

		public override Task ClearCatalogAsync() => throw new System.NotImplementedException();

		public override void DeleteSchema(string objectName) => throw new System.NotImplementedException();

		public override Task DeleteSchemaAsync(string objectName) => throw new System.NotImplementedException();

		public override IEnumerable<string> GetChildObjects(string objectName) => throw new System.NotImplementedException();

		public override JMXSchema GetSchema(string objectName)
		{
			JMXSchema schema = ClientGate.GetObjectSchema(objectName);
			schema.SchemaRepo = this;
			return schema;
		}

		public override Task<JMXSchema> GetSchemaAsync(string objectName) => throw new System.NotImplementedException();

		public override JMXSchema SaveSchema(JMXSchema schema) => throw new System.NotImplementedException();

		public override Task<JMXSchema> SaveSchemaAsync(JMXSchema schema) => throw new System.NotImplementedException();

		public override Task<JMXSchema> SetSchemaStateAsync(string objectName, int stateId) => throw new System.NotImplementedException();
	}
}
