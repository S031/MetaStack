using System;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Data;
using S031.MetaStack.ORM;

namespace S031.MetaStack.Core.ORM.MsSql
{
	[DBRef(DBProviderName = "System.Data.SqlClient")]
	public class JMXSqlFactory: JMXFactory
	{
		public const string ProviderInvariantName = "System.Data.SqlClient";

		private JMXSqlRepo _repo;
		private readonly JMXSqlProvider _jmx;

		public JMXSqlFactory(IServiceProvider services, MdbContext workMdbContext)
		{
			string providerName = workMdbContext.ProviderName;
			if (!providerName
				.Equals(ProviderInvariantName, StringComparison.OrdinalIgnoreCase))
				throw new ArgumentException($"MdbContext must be created using { ProviderInvariantName} provider.");
			_jmx = new JMXSqlProvider(this);
		}

		public override IJMXRepo CreateJMXRepo()
		{
			if (_repo == null)
				_repo = new JMXSqlRepo(this);
			return _repo;
		}

		public override IJMXProvider CreateJMXProvider() => _jmx;

		public override JMXObject CreateObject(string objectName) => new JMXObject(objectName, this);

		public override SQLStatementWriter CreateSQLStatementWriter() => new SQLStatementWriter(new JMXSQLTypeMapping());
	}
}
