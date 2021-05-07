using System;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Data;
using S031.MetaStack.ORM;

namespace S031.MetaStack.Core.ORM.SQLite
{
	[DBRef(DBProviderName = "System.Data.Sqlite")]
	public class JMXSQLiteFactory: JMXFactory
	{
		public const string ProviderInvariantName = "System.Data.Sqlite";

		private JMXSQLiteRepo _repo;
		private readonly JMXSQLiteProvider _jmx;

		public JMXSQLiteFactory(IServiceProvider services, MdbContext workMdbContext)
		{
			string providerName = workMdbContext.ProviderName;
			if (!providerName
				.Equals(ProviderInvariantName, StringComparison.OrdinalIgnoreCase))
				throw new ArgumentException($"MdbContext must be created using { ProviderInvariantName} provider.");
			_jmx = new JMXSQLiteProvider(this);
		}

		public override IJMXRepo CreateJMXRepo()
		{
			if (_repo == null)
				_repo = new JMXSQLiteRepo(this);
			return _repo;
		}

		public override IJMXProvider CreateJMXProvider() => _jmx;

		public override JMXObject CreateObject(string objectName) => new JMXObject(objectName, this);

		public override SQLStatementWriter CreateSQLStatementWriter() => new SQLStatementWriter(new JMXSQLiteTypeMapping(), false);

		public override IJMXBalance CreateJMXBalance() => new JMXSQLiteBalance(this);
	}
}
