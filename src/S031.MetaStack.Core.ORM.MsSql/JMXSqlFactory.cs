using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;

namespace S031.MetaStack.Core.ORM.MsSql
{
	[SchemaDBSync(DBProviderName = "System.Data.SqlClient")]
	public class JMXSqlFactory: JMXFactory
	{
		public const string ProviderInvariantName = "System.Data.SqlClient";

		private readonly JMXSqlRepo _repo;
		private readonly JMXSqlProvider _jmx;
		private readonly JMXFactory _schemaFactory;
		public JMXSqlFactory(MdbContext mdbContext, ILogger logger) : this(mdbContext, mdbContext, logger)
		{
		}

		public JMXSqlFactory(MdbContext sysCatMdbContext, MdbContext workMdbContext, ILogger logger) : base(sysCatMdbContext, workMdbContext)
		{
			string providerName = workMdbContext.ProviderName;
			if (!providerName
				.Equals(ProviderInvariantName, StringComparison.OrdinalIgnoreCase))
				throw new ArgumentException($"MdbContext must be created using { ProviderInvariantName} provider.");
			this.Logger = logger;
			_repo = new JMXSqlRepo(this);
			_jmx = new JMXSqlProvider(this);
			_schemaFactory = providerName.Equals(sysCatMdbContext.ProviderName, StringComparison.OrdinalIgnoreCase) ?
				this :
				JMXFactory.Create(sysCatMdbContext, logger);
		}

		public override IJMXRepo CreateJMXRepo() => _repo;

		public override IJMXProvider CreateJMXProvider() => _jmx;

		public override JMXObject CreateObject(string objectName) => new JMXObject(objectName, this);

		public override SQLStatementWriter CreateSQLStatementWriter() => new SQLStatementWriter(new JMXSQLTypeMapping());

		public override IJMXFactory SchemaFactory => _schemaFactory;
	}
}
