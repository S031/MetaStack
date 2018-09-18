using System;
using System.Collections.Generic;
using System.Text;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;

namespace S031.MetaStack.Core.ORM
{
	public class JMXSqlProvider : JMXProvider
	{
		public JMXSqlProvider(MdbContext mdbContext, Microsoft.Extensions.Logging.ILogger logger) : base(mdbContext)
		{
			if (!mdbContext.ProviderName.Equals(JMXSqlFactory.ProviderInvariantName, StringComparison.CurrentCultureIgnoreCase))
				throw new ArgumentException($"MdbContext must be created using { JMXSqlFactory.ProviderInvariantName} provider.");
			this.Logger = logger;
		}

	}
}
