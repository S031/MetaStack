using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using S031.MetaStack.Core.App;
using S031.MetaStack.Data;
using S031.MetaStack.Actions;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace S031.MetaStack.Core.Actions
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

