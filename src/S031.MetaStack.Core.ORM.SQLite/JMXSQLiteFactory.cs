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

		private readonly JMXSQLiteRepo _repo;
		private readonly JMXSQLiteProvider _jmx;
		private readonly JMXFactory _schemaFactory;
		public JMXSQLiteFactory(MdbContext mdbContext, ILogger logger) : this(mdbContext, mdbContext, logger)
		{
		}

		public JMXSQLiteFactory(MdbContext sysCatMdbContext, MdbContext workMdbContext, ILogger logger) : base(sysCatMdbContext, workMdbContext)
		{
			string providerName = workMdbContext.ProviderName;
			if (!providerName
				.Equals(ProviderInvariantName, StringComparison.OrdinalIgnoreCase))
				throw new ArgumentException($"MdbContext must be created using { ProviderInvariantName} provider.");
			this.Logger = logger;
			_repo = new JMXSQLiteRepo(this);
			_jmx = new JMXSQLiteProvider(this);
			_schemaFactory = providerName.Equals(sysCatMdbContext.ProviderName, StringComparison.OrdinalIgnoreCase) ?
				this :
				JMXFactory.Create(sysCatMdbContext, logger);
		}

		public override IJMXRepo CreateJMXRepo() => _repo;

		public override IJMXProvider CreateJMXProvider() => _jmx;

		public override JMXObject CreateObject(string objectName) => new JMXObject(objectName, this);

		public override SQLStatementWriter CreateSQLStatementWriter() => new SQLStatementWriter(new JMXSQLiteTypeMapping(), false);

		public override IJMXFactory SchemaFactory => _schemaFactory;
	}
}
