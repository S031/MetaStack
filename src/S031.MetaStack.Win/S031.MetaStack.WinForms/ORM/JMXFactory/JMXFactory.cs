using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.WinForms.ORM
{
	public abstract class JMXFactory : IJMXFactory
	{
		public virtual IJMXProvider CreateJMXProvider()
		{
			throw new NotImplementedException();
		}

		public virtual IJMXRepo CreateJMXRepo()
		{
			throw new NotImplementedException();
		}

		public virtual JMXObject CreateObject(string objectName)
		{
			throw new NotImplementedException();
		}

		public static IJMXFactory Create()
		{
			return new JMXFactoryClient();
		}
	}

	public class JMXFactoryClient : JMXFactory
	{

	}
}
