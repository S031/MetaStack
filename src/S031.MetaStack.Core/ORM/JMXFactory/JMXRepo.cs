using S031.MetaStack.ORM;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace S031.MetaStack.Core.ORM
{
	public abstract class JMXRepo : IJMXRepo
	{
		private readonly JMXFactory _factory;
		
		public JMXRepo(JMXFactory factory)
		{
			_factory = factory;
		}
		
		protected virtual JMXFactory Factory => _factory;

		public virtual IEnumerable<string> GetChildObjects(string objectName)
		{
			throw new NotImplementedException();
		}

		public abstract JMXSchema GetSchema(string objectName);

		public abstract JMXSchema SaveSchema(JMXSchema schema);

		public abstract void DropSchema(string objectName);

		public abstract void ClearCatalog();

		public abstract Task<JMXSchema> GetSchemaAsync(string objectName);

		public abstract Task<JMXSchema> SaveSchemaAsync(JMXSchema schema);

		public abstract Task<JMXSchema> SyncSchemaAsync(string objectName);

		public abstract Task DropSchemaAsync(string objectName);

		public abstract Task ClearCatalogAsync();

		public virtual Task<JMXSchema> GetTableSchemaAsync(string objectName)
			=> throw new NotImplementedException();
	}
}
