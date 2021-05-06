using S031.MetaStack.Data;
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
		
		protected JMXFactory Factory => _factory;

		public abstract void ClearCatalog();

		public abstract Task ClearCatalogAsync();

		public abstract void DeleteSchema(string objectName);

		public abstract Task DeleteSchemaAsync(string objectName);

		public abstract IEnumerable<string> GetChildObjects(string objectName);

		public abstract JMXSchema GetSchema(string objectName);

		public abstract Task<JMXSchema> GetSchemaAsync(string objectName);

		public abstract JMXSchema SaveSchema(JMXSchema schema);

		public abstract Task<JMXSchema> SaveSchemaAsync(JMXSchema schema);
	}
}
