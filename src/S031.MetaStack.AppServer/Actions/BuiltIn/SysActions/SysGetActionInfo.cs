using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Actions;
using S031.MetaStack.Data;
using System;
using System.Threading.Tasks;

namespace S031.MetaStack.AppServer.Actions
{
	internal class SysGetActionInfo : IAppEvaluator
	{

		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			IServiceProvider services = ai.GetContext().Services;
			string actionID = string.Empty; ;
			if (dp.Read())
				actionID = (string)dp["ActionID"];

			IActionManager am = services.GetRequiredService<IActionManager>();
			return new DataPackage(
				new string[] { "ActionInfo" },
				new object[] { am.GetActionInfo(actionID)?.ToString() });
		}

		public async Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			IServiceProvider services = ai.GetContext().Services;
			string actionID = string.Empty; ;
			if (dp.Read())
				actionID = (string)dp["ActionID"];

			IActionManager am = services.GetRequiredService<IActionManager>();
			return new DataPackage(
				new string[] { "ActionInfo" },
				new object[] { (await am.GetActionInfoAsync(actionID))?.ToString() });
		}
	}
}

