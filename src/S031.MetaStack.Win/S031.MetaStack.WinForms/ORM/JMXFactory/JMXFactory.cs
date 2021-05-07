using S031.MetaStack.ORM;
using System;

namespace S031.MetaStack.WinForms.ORM
{
	public abstract class JMXFactory : IJMXFactory
	{
		private static readonly IJMXFactory _factory = new JMXClientFactory();

		public virtual IJMXFactory SchemaFactory => _factory;

		public abstract IJMXProvider CreateJMXProvider();

		public abstract IJMXRepo CreateJMXRepo();

		public abstract JMXObject CreateObject(string objectName);

		public static IJMXFactory Create() => _factory;

		public abstract IJMXTypeMapping CreateJMXTypeMapping();

		public abstract IJMXBalance CreateJMXBalance();
	}
}
