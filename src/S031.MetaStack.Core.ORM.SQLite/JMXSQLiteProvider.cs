using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;

namespace S031.MetaStack.Core.ORM.SQLite
{
	public class JMXSQLiteProvider : JMXProvider
	{
		public JMXSQLiteProvider(MdbContext sysCatMdbContext, MdbContext workMdbContext, ILogger logger) : base(sysCatMdbContext, workMdbContext)
		{
			if (!sysCatMdbContext.ProviderName.Equals(JMXSQLiteFactory.ProviderInvariantName, StringComparison.CurrentCultureIgnoreCase))
				throw new ArgumentException($"MdbContext must be created using { JMXSQLiteFactory.ProviderInvariantName} provider.");
			this.Logger = logger;
		}

	}
}
