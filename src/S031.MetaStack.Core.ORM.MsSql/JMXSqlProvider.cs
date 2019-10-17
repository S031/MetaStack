using System;
using System.Collections.Generic;
using System.Text;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace S031.MetaStack.Core.ORM.MsSql
{
	public class JMXSqlProvider : JMXProvider
	{
		public JMXSqlProvider(JMXSqlFactory factory) : base(factory)
		{
		}

		public override async Task<JMXObject> ReadAsync(JMXObjectName objectName, int id)
		{
			JMXObject o = new JMXObject(objectName, Factory);
			if (!_statementsCache.TryGetValue(objectName, out string sql))
			{
				//Factory
				//	.CreateSQLStatementWriter()
				//	.WriteSelectStatement()
				_statementsCache.TryAdd(objectName, sql);
			}
			var mdb = Factory.GetMdbContext(ContextTypes.Work);
			var drs = await mdb.GetReadersAsync(sql);
			for (int i = 0; i < drs.Length; i++)
			{
				var dr = drs[i];
				dr.Dispose();
			}
			return o;
		}

	}
}