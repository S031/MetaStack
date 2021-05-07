using S031.MetaStack.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.ORM
{
	public interface IJMXRepo
	{
		JMXSchema GetSchema(string objectName);
		Task<JMXSchema> GetSchemaAsync(string objectName);
		JMXSchema SaveSchema(JMXSchema schema);
		Task<JMXSchema> SaveSchemaAsync(JMXSchema schema);
		void DeleteSchema(string objectName);
		Task DeleteSchemaAsync(string objectName);
		void ClearCatalog();
		Task ClearCatalogAsync();

		//!!! Refactor this
		Task<JMXSchema> SetSchemaStateAsync(string objectName, int stateId);
	}
}
