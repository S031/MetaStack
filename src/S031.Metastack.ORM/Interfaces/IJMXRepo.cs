using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.ORM
{
	public interface IJMXRepo
	{
		JMXSchema GetSchema(string objectName);
		JMXSchema SaveSchema(JMXSchema schema);
		void DropSchema(string objectName);
		void ClearCatalog();
		Task<JMXSchema> GetSchemaAsync(string objectName);
		Task<JMXSchema> SaveSchemaAsync(JMXSchema schema);
		Task<JMXSchema> SyncSchemaAsync(string objectName);
		Task DropSchemaAsync(string objectName);
		Task ClearCatalogAsync();
		Task<JMXSchema> GetTableSchemaAsync(string objectName);
	}
}
