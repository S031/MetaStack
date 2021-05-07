using S031.MetaStack.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.WinForms.ORM
{
	public abstract class JMXBalance : IJMXBalance
	{
		public abstract void DropObjectSchema(string objectName);

		public abstract Task DropObjectSchemaAsync(string objectName);

		public abstract JMXSchema GetObjectSchema(string objectName);

		public abstract Task<JMXSchema> GetObjectSchemaAsync(string objectName);

		public abstract JMXSchema SyncObjectSchema(string objectName);

		public abstract Task<JMXSchema> SyncObjectSchemaAsync(string objectName);
	}
}
