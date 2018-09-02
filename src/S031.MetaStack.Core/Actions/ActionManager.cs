using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Core.Actions
{
	public class ActionManager:IDisposable
	{
		private static readonly object obj4Lock = new object();
		private static readonly Dictionary<string, ActionInfo> actions = new Dictionary<string, ActionInfo>();
		private MdbContext _mdbContext;

		ActionManager() { }

		public ActionManager(string sysCatConnectionName, ILogger logger)
		{
			var _configuration = ApplicationContext.GetServices().GetService<IConfiguration>();
			var cs = _configuration.GetSection($"connectionStrings:{sysCatConnectionName}")?.Get<ConnectInfo>();
			if (cs == null)
				throw new KeyNotFoundException($"The connection string with name {sysCatConnectionName} was not found in the configuration file");
			_mdbContext = new MdbContext(MdbContext.CreateConnectionString(cs.ProviderName, cs.ConnectionString));
		}

		public static async Task<ActionManager> CreateManagerAsync(string sysCatConnectionName, ILogger logger)
		{
			ActionManager actionManager = new ActionManager
			{
				_mdbContext = await MdbContext.CreateMdbContextAsync(sysCatConnectionName)
			};
			return actionManager;
		}

		internal static Dictionary<string, ActionInfo> Actions => actions;

		public void Dispose()
		{
			_mdbContext.Dispose();
		}
	}
}
