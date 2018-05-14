using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

#if NETCOREAPP2_0
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public interface IJMXSchemaProvider
    {
		IEnumerable<string> GetChildObjects(string objectName);
		JMXSchema GetSchema(string objectName);
		JMXSchema SaveSchema(JMXSchema schema);
		void DropSchema(string objectName);
		Task<JMXSchema> GetSchemaAsync(string objectName);
		Task<JMXSchema> SaveSchemaAsync(JMXSchema schema);
		Task<JMXSchema> SyncSchemaAsync(string objectName);
		Task DropSchemaAsync(string objectName);
	}
}
