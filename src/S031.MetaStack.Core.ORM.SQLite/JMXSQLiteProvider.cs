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
		private readonly JMXSQLiteFactory _factory;
		public JMXSQLiteProvider(JMXSQLiteFactory factory) : base(factory)
		{
			_factory = factory;
		}

	}
}
