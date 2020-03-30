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
		public virtual void ClearCatalog()
		{
			throw new NotImplementedException();
		}

		public virtual Task ClearCatalogAsync()
		{
			throw new NotImplementedException();
		}

		public virtual void DropSchema(string objectName)
		{
			throw new NotImplementedException();
		}

		public virtual Task DropSchemaAsync(string objectName)
		{
			throw new NotImplementedException();
		}

		public virtual IEnumerable<string> GetChildObjects(string objectName)
		{
			throw new NotImplementedException();
		}

		public virtual JMXSchema GetSchema(string objectName)
		{
			throw new NotImplementedException();
		}

		public virtual Task<JMXSchema> GetSchemaAsync(string objectName)
		{
			throw new NotImplementedException();
		}

		public Task<JMXSchema> GetTableSchemaAsync(string objectName)
		{
			throw new NotImplementedException();
		}

		public bool IsSchemaSupport() => true;

		public virtual JMXSchema SaveSchema(JMXSchema schema)
		{
			throw new NotImplementedException();
		}

		public virtual Task<JMXSchema> SaveSchemaAsync(JMXSchema schema)
		{
			throw new NotImplementedException();
		}

		public virtual Task<JMXSchema> SyncSchemaAsync(string objectName)
		{
			throw new NotImplementedException();
		}
	}
}
