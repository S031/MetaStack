using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Actions;
using S031.MetaStack.Common;
using S031.MetaStack.Data;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Metib.Factoring.Clients.CKS
{
	public class CKSTest : IAppEvaluator
	{
		public DataPackage Invoke(ActionInfo ai, DataPackage dp) => InvokeInternalAsync(ai, dp).GetAwaiter().GetResult();

		public async Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
			=> await InvokeInternalAsync(ai, dp);

		public async Task<DataPackage> InvokeInternalAsync(ActionInfo ai, DataPackage dp)
		{
			var ctx = ai.GetContext();
			string totals = string.Empty;

			int loopCount = ((string)dp["@IDs"]).Split(',')[0].ToIntOrDefault();
			if (loopCount > 0)
			{
				var start = DateTime.Now;
				var am = ai.GetContext()
					.Services
					.GetRequiredService<IActionManager>();
				for (int i = 0; i < loopCount; i++)
				{
					await am.ExecuteAsync(ai, ai.GetInputParamTable()
						.AddNew()
						.SetValue("@ObjectName", "Test")
						.SetValue("@IDs", "0")
						.Update());
				}
				totals = $"IActionManager.ExecuteAsync test loop count = {loopCount}, total times = {(DateTime.Now - start).TotalMilliseconds}ms";
			}
			var result = ai.GetOutputParamTable()
				.AddNew()
				.SetValue("@Result", totals)
				.Update();
			return result;
		}
	}
}
