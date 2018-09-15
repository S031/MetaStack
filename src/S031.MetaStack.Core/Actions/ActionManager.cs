﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using S031.MetaStack.Common;

namespace S031.MetaStack.Core.Actions
{
	public class ActionManager:IDisposable
	{
		private static readonly object obj4Lock = new object();
		//private static readonly Dictionary<string, ActionInfo> actions = new Dictionary<string, ActionInfo>();
		private MdbContext _mdbContext;

		ActionManager() { }

		public ActionManager(string sysCatConnectionName, ILogger logger)
		{
			var _configuration = ApplicationContext.GetServices().GetService<IConfiguration>();
			var cs = _configuration.GetSection($"connectionStrings:{sysCatConnectionName}")?.Get<ConnectInfo>();
			if (cs == null)
				throw new KeyNotFoundException($"The connection string with name {sysCatConnectionName} was not found in the configuration file");
			_mdbContext = new MdbContext(new ConnectInfo(cs.ProviderName, cs.ConnectionString));
		}

		public static async Task<ActionManager> CreateManagerAsync(string sysCatConnectionName, ILogger logger)
		{
			ActionManager actionManager = new ActionManager
			{
				_mdbContext = await MdbContext.CreateMdbContextAsync(sysCatConnectionName)
			};
			return actionManager;
		}

		internal static Dictionary<string, ActionInfo> Actions => _actions;

		public void Dispose()
		{
			_mdbContext.Dispose();
		}

		private static readonly Dictionary<string, ActionInfo> _actions = new Dictionary<string, ActionInfo>()
		{
			{"Sys.LoginRequest", new ActionInfo(){
				ActionID = "Sys.LoginRequest",
				AssemblyID = Assembly.GetExecutingAssembly().GetWorkName(),
				}
			}
		};
	}
}
