using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace S031.MetaStack.Core.ORM
{
	public abstract class JMXFactory : ManagerObjectBase, IJMXFactory, IDisposable
	{
		public JMXFactory(MdbContext mdbContext) : base(mdbContext)
		{
		}
		public virtual IJMXRepo CreateJMXRepo()
		{
			throw new NotImplementedException();
		}
		public virtual IJMXProvider CreateJMXProvider()
		{
			throw new NotImplementedException();
		}
		public virtual JMXObject CreateObject(string objectName)
		{
			throw new NotImplementedException();
		}
		public static JMXFactory Create(MdbContext mdb, ILogger logger)
		{
			var l = ImplementsList.GetTypes(typeof(JMXFactory));
			if (l == null)
				throw new InvalidOperationException("No class inherited from JMXFactory defined");
			string dbProviderName = mdb.ProviderName.ToLower();
			foreach (var t in l)
			{
				SchemaDBSyncAttribute att = (System.Attribute.GetCustomAttributes(t)?
					.FirstOrDefault(attr => attr.GetType() == typeof(SchemaDBSyncAttribute) &&
					(attr as SchemaDBSyncAttribute)?.DBProviderName.ToLower() == dbProviderName) as SchemaDBSyncAttribute);
				if (att != null)
					return (JMXFactory)t.CreateInstance(mdb, logger);
			}
			throw new InvalidOperationException("No class inherited from JMXFactory contained attribute of type SchemaDBSyncAttribute  defined");
		}
	}
}
