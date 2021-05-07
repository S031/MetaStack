using S031.MetaStack.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.WinForms.ORM
{
	public abstract class JMXRepo : IJMXRepo
	{
		public abstract void ClearCatalog();

		public abstract Task ClearCatalogAsync();

		public abstract void DeleteSchema(string objectName);

		public abstract Task DeleteSchemaAsync(string objectName);

		public abstract IEnumerable<string> GetChildObjects(string objectName);

		public abstract JMXSchema GetSchema(string objectName);

		public abstract Task<JMXSchema> GetSchemaAsync(string objectName);

		public virtual bool IsSchemaSupport() => true;

		public abstract JMXSchema SaveSchema(JMXSchema schema);

		public abstract Task<JMXSchema> SaveSchemaAsync(JMXSchema schema);

		public abstract Task<JMXSchema> SetSchemaStateAsync(string objectName, int stateId);
	}
}
