using System;
using System.Collections.Generic;
using System.Text;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;

namespace S031.MetaStack.Core.ORM.SQLite
{
	public class JMXSQLiteProvider : JMXProvider
	{
		public JMXSQLiteProvider(MdbContext mdbContext, Microsoft.Extensions.Logging.ILogger logger) : base(mdbContext)
		{
			if (!mdbContext.ProviderName.Equals(JMXSQLiteFactory.ProviderInvariantName, StringComparison.CurrentCultureIgnoreCase))
				throw new ArgumentException($"MdbContext must be created using { JMXSQLiteFactory.ProviderInvariantName} provider.");
			this.Logger = logger;
		}

	}
}
