using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Common;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Data;
using S031.MetaStack.Core.ORM;

namespace S031.MetaStack.Core.Actions
{
	internal class SysGetActionInfo : IAppEvaluator
	{
		ConnectInfo _connectInfo;
		string _actionID;

		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			GetParameters(ai, dp);
			using (MdbContext mdb = new MdbContext(_connectInfo))
			using (ActionManager am = new ActionManager(mdb) { Logger = ApplicationContext.GetLogger() })
			{
				return new DataPackage(new string[] { "ActionInfo" },
					new object[] { am.GetActionInfo(_actionID)?.ToString() });
			}
		}

		public async Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			GetParameters(ai, dp);
			//using (MdbContext mdb = await MdbContext.CreateMdbContextAsync(_connectInfo))
			using (MdbContext mdb = new MdbContext(_connectInfo))
			using (ActionManager am = new ActionManager(mdb) { Logger = ApplicationContext.GetLogger() })
			{
				return new DataPackage(new string[] { "ActionInfo" },
					new object[] { (await am.GetActionInfoAsync(_actionID)).ToString() });
			}
		}

		private void GetParameters(ActionInfo ai, DataPackage dp)
		{
			var configuration = ApplicationContext.GetConfiguration();
			if (!dp.Headers.TryGetValue("ConnectionName", out object connectionName))
				connectionName = configuration["appSettings:SysCatConnection"];
			_connectInfo = configuration.GetSection($"connectionStrings:{connectionName}").Get<ConnectInfo>();

			if (dp.Read())
				_actionID = (string)dp["ActionID"];
		}
	}
}

