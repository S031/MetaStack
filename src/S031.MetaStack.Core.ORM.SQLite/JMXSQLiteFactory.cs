using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;

namespace S031.MetaStack.Core.ORM.SQLite
{
	[SchemaDBSync(DBProviderName = "Microsoft.Data.Sqlite")]
	public class JMXSQLiteFactory: JMXFactory
	{
		public const string ProviderInvariantName = "Microsoft.Data.Sqlite";

		private readonly JMXSQLiteRepo _repo;
		private readonly JMXSQLiteProvider _jmx;
		public JMXSQLiteFactory(MdbContext mdbContext, ILogger logger) : base(mdbContext)
		{
			if (!mdbContext.ProviderName.Equals(ProviderInvariantName, StringComparison.CurrentCultureIgnoreCase))
				throw new ArgumentException($"MdbContext must be created using { ProviderInvariantName} provider.");
			this.Logger = logger;
			_repo = new JMXSQLiteRepo(mdbContext, logger);
			_jmx = new JMXSQLiteProvider(mdbContext, logger);
		}
		public override IJMXRepo CreateJMXRepo() => _repo;

		public override IJMXProvider CreateJMXProvider() => _jmx;

		public override JMXObject CreateObject(string objectName)
		{
			return new JMXObject(objectName, this);
		}
	}
}
