using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.ORM
{
	public interface IJMXBalance
	{
		Task<JMXSchema> GetObjectSchemaAsync(string objectName);
		JMXSchema GetObjectSchema(string objectName);
		Task DropObjectSchemaAsync(string objectName);
		void DropObjectSchema(string objectName);
		Task<JMXSchema> SyncObjectSchemaAsync(string objectName);
		JMXSchema SyncObjectSchema(string objectName);

	}
}
