using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public interface IJMXRepo
	{
		IEnumerable<string> GetChildObjects(string objectName);
		JMXSchema GetSchema(string objectName);
		JMXSchema SaveSchema(JMXSchema schema);
		void DropSchema(string objectName);
		void ClearCatalog();
		Task<JMXSchema> GetSchemaAsync(string objectName);
		Task<JMXSchema> SaveSchemaAsync(JMXSchema schema);
		Task<JMXSchema> SyncSchemaAsync(string objectName);
		Task DropSchemaAsync(string objectName);
		Task ClearCatalogAsync();
		bool IsSchemaSupport();
	}
}
