using System;
using System.Collections.Generic;
using System.Text;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;
using Microsoft.Extensions.Logging;

namespace S031.MetaStack.Core.ORM.MsSql
{
	public class JMXSqlProvider : JMXProvider
	{
		public JMXSqlProvider(JMXSqlFactory factory) : base(factory)
		{
		}
	}
}
