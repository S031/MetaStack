using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;

namespace S031.MetaStack.Core.ORM
{
	[SchemaDBSync(DBProviderName = "System.Data.SqlClient")]
	public class JMXSqlFactory: JMXFactory
	{
		public const string ProviderInvariantName = "System.Data.SqlClient";

		private readonly JMXSqlRepo _repo;
		private readonly JMXSqlProvider _jmx;
		public JMXSqlFactory(MdbContext mdbContext, ILogger logger) : base(mdbContext)
		{
			if (!mdbContext.ProviderName.Equals(ProviderInvariantName, StringComparison.CurrentCultureIgnoreCase))
				throw new ArgumentException($"MdbContext must be created using { ProviderInvariantName} provider.");
			this.Logger = logger;
			_repo = new JMXSqlRepo(mdbContext, logger);
			_jmx = new JMXSqlProvider(mdbContext, logger);
		}
		public override IJMXRepo CreateJMXRepo() => _repo;

		public override IJMXProvider CreateJMXProvider() => _jmx;

		public override JMXObject CreateObject(string objectName)
		{
			return new JMXObject(objectName, this);
		}
	}
}
