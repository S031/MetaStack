using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Data;
using S031.MetaStack.ORM;
using System;
using System.Linq;

namespace S031.MetaStack.Core.ORM
{
	public abstract class JMXFactory : ManagerObjectBase, IJMXFactory, IDisposable
	{
		public JMXFactory(MdbContext mdbContext) : base(mdbContext)
		{
		}

		public JMXFactory(MdbContext sysCatMdbContext, MdbContext workMdbContext) : base(sysCatMdbContext, workMdbContext)
		{
		}

		public virtual IJMXFactory SchemaFactory => throw new NotImplementedException();

		public virtual IJMXRepo CreateJMXRepo() => throw new NotImplementedException();

		public virtual IJMXProvider CreateJMXProvider() => throw new NotImplementedException();

		public virtual JMXObject CreateObject(string objectName) => throw new NotImplementedException();

		public static JMXFactory Create(MdbContext mdb, ILogger logger) => Create(mdb, mdb, logger);

		public static JMXFactory Create(MdbContext sysCatMdbContext, MdbContext workMdbContext, ILogger logger)
		{
			var l = ImplementsList.GetTypes(typeof(JMXFactory));
			if (l == null)
				throw new InvalidOperationException("No class inherited from JMXFactory defined");
			string dbProviderName = workMdbContext.ProviderName;
			foreach (var t in l)
			{
				if (System.Attribute.GetCustomAttributes(t)?
					.FirstOrDefault(attr => attr.GetType() == typeof(DBRefAttribute)
						&& (attr as DBRefAttribute)
							.DBProviderName
							.Equals(dbProviderName, StringComparison.OrdinalIgnoreCase)) is DBRefAttribute att)
					return (JMXFactory)t.CreateInstance(sysCatMdbContext, workMdbContext, logger);
			}
			throw new InvalidOperationException("No class inherited from JMXFactory contained attribute of type DBRefAttribute  defined");
		}

		public virtual SQLStatementWriter CreateSQLStatementWriter() => new SQLStatementWriter(new JMXTypeMappingAnsi());

		public virtual IJMXTypeMapping CreateJMXTypeMapping() => new JMXTypeMappingAnsi();
	}
}
