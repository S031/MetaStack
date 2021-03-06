﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Actions;
using S031.MetaStack.Core.ORM;
using S031.MetaStack.Core.Properties;
using S031.MetaStack.Data;

namespace S031.MetaStack.Core.Actions
{
	public static class ActionContextExtensions
	{
		public static JMXFactory CreateJMXFactory(this ActionContext ctx, string workConnectionName)
		{
			var services = ctx.Services;
			var mdbFactory = services.GetRequiredService<IMdbContextFactory>();
			MdbContext workDb = mdbFactory.GetContext(workConnectionName);
			var f = JMXFactory.Create(services, workDb);
			return f;
		}
	}
}
