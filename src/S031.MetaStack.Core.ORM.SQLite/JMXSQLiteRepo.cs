using Microsoft.Extensions.Logging;
using S031.MetaStack.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Core.ORM.SQLite
{
	public class JMXSQLiteRepo : JMXRepo
	{
		public JMXSQLiteRepo(MdbContext mdbContext, ILogger logger) : base(mdbContext)
		{
			if (!mdbContext.ProviderName.Equals(JMXSQLiteFactory.ProviderInvariantName, StringComparison.CurrentCultureIgnoreCase))
				throw new ArgumentException($"MdbContext must be created using { JMXSQLiteFactory.ProviderInvariantName} provider.");
			Logger = logger;
		}
	}
}