using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.WinForms.ORM
{
	public class JMXClientFactory : JMXFactory
	{
		static IJMXRepo _repo = new JMXClientRepo();

		static IJMXProvider _provider = new JMXClientProvider();

		public override IJMXRepo CreateJMXRepo() => _repo;

		public override IJMXProvider CreateJMXProvider() => _provider;

		public override JMXObject CreateObject(string objectName) =>
			new JMXObject(objectName, this);
	}
}
