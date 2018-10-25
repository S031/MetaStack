using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.WinForms.ORM
{
	public class JMXClientFactory : JMXFactory
	{
		public override IJMXRepo CreateJMXRepo() =>
			new JMXClientRepo();

		public override IJMXProvider CreateJMXProvider() =>
			new JMXClientProvider();

		public override JMXObject CreateObject(string objectName) =>
			new JMXObject(objectName, this);
	}
}
