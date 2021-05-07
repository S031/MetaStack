using S031.MetaStack.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.WinForms.ORM
{
	public class JMXClientBalance : JMXBalance
	{
		public override void DropObjectSchema(string objectName) => throw new NotImplementedException();

		public override Task DropObjectSchemaAsync(string objectName) => throw new NotImplementedException();

		public override JMXSchema GetObjectSchema(string objectName) => throw new NotImplementedException();

		public override Task<JMXSchema> GetObjectSchemaAsync(string objectName) => throw new NotImplementedException();

		public override JMXSchema SyncObjectSchema(string objectName) => throw new NotImplementedException();

		public override Task<JMXSchema> SyncObjectSchemaAsync(string objectName) => throw new NotImplementedException();
	}
}
