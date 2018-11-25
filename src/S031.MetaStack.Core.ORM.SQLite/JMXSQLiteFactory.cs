using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;

namespace S031.MetaStack.Core.ORM.SQLite
{
	[SchemaDBSync(DBProviderName = "System.Data.Sqlite")]
	public class JMXSQLiteFactory: JMXFactory
	{
		public const string ProviderInvariantName = "System.Data.Sqlite";

		private readonly JMXSQLiteRepo _repo;
		private readonly JMXSQLiteProvider _jmx;
		public JMXSQLiteFactory(MdbContext mdbContext, ILogger logger) : this(mdbContext, mdbContext, logger)
		{
		}

		public JMXSQLiteFactory(MdbContext sysCatMdbContext, MdbContext workMdbContext, ILogger logger) : base(sysCatMdbContext, workMdbContext)
		{
			if (!sysCatMdbContext.ProviderName.Equals(ProviderInvariantName, StringComparison.CurrentCultureIgnoreCase))
				throw new ArgumentException($"MdbContext must be created using { ProviderInvariantName} provider.");
			this.Logger = logger;
			_repo = new JMXSQLiteRepo(sysCatMdbContext, workMdbContext, logger);
			_jmx = new JMXSQLiteProvider(sysCatMdbContext, workMdbContext, logger);
		}

		public override IJMXRepo CreateJMXRepo() => _repo;

		public override IJMXProvider CreateJMXProvider() => _jmx;

		public override JMXObject CreateObject(string objectName) => new JMXObject(objectName, this);

		public override SQLStatementWriter CreateSQLStatementWriter() => new SQLStatementWriter(new JMXSQLiteTypeMapping());
	}
}
