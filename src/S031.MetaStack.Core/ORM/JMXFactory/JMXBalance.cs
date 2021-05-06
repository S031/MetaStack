using S031.MetaStack.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Core.ORM
{
	public abstract class JMXBalance : IJMXBalance
	{
		private readonly JMXFactory _factory;
		protected JMXFactory Factory => _factory;

		public JMXBalance(JMXFactory factory)
		{
			_factory = factory;
		}

		public abstract void DropObjectSchema(string objectName);

		public abstract Task DropObjectSchemaAsync(string objectName);

		public abstract JMXSchema GetObjectSchema(string objectName);

		public abstract Task<JMXSchema> GetObjectSchemaAsync(string objectName);

		public abstract JMXSchema SyncObjectSchema(string objectName);

		public abstract Task<JMXSchema> SyncObjectSchemaAsync(string objectName);
	}
}
