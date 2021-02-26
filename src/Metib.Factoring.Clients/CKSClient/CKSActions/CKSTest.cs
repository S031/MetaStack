using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Actions;
using S031.MetaStack.Common;
using S031.MetaStack.Data;
using S031.MetaStack.Integral.Settings;
using S031.MetaStack.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaskPlus.Server.Api.Settings;

namespace Metib.Factoring.Clients.CKS
{
	public class CKSTest : IAppEvaluator
	{
		public DataPackage Invoke(ActionInfo ai, DataPackage dp) => InvokeInternalAsync(ai, dp).GetAwaiter().GetResult();

		public async Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
			=> await InvokeInternalAsync(ai, dp);

		public async Task<DataPackage> InvokeInternalAsync(ActionInfo ai, DataPackage dp)
		{
			//var ctx = ai.GetContext();
			string totals = string.Empty;

			string testName = (string)dp["@ObjectName"];
			int loopCount = ((string)dp["@IDs"]).Split(',')[0].ToIntOrDefault();
			
			switch (testName)
			{

				case "Test":
					totals = BlankProcedureForCallSpeedTests();
					break;
				case "RecursiveCallExecuteAsync":
					totals = await RecursiveCallExecuteAsync(ai, loopCount);
					break;
				case "GetSettingsSpeedTest":
					totals = await GetSettingsSpeedTest(ai, loopCount);
					break;
			}
			
			return ai.GetOutputParamTable()
				.AddNew()
				.SetValue("@Result", totals)
				.Update();
		}

		private static string BlankProcedureForCallSpeedTests()
			=> "Test completed";

		private static async Task<string> GetSettingsSpeedTest(ActionInfo ai, int loopCount)
		{
			IServiceProvider services = ai.GetContext().Services;
			ISettingsProvider<VocabularySettingsProvider> settings
				= services.GetRequiredService<ISettingsProvider<VocabularySettingsProvider>>();

			var start = DateTime.Now;
			JsonObject rabbitConnectorOptions = null;
			for (int i = 0; i < loopCount; i++)
				rabbitConnectorOptions = JsonObject.Parse(await settings.GetSettings("CKSServiceSettings.RabbitConnectorOptions"));
			return $"GetSettingsSpeedTest loop count = {loopCount}, total times = {(DateTime.Now - start).TotalMilliseconds}ms\n{rabbitConnectorOptions}";
		}

		private static async Task<string> RecursiveCallExecuteAsync(ActionInfo ai, int loopCount)
		{
			if (loopCount > 0)
			{
				var am = ai.GetContext()
					.Services
					.GetRequiredService<IActionManager>();
				
				var start = DateTime.Now;
				for (int i = 0; i < loopCount; i++)
				{
					await am.ExecuteAsync(ai, ai.GetInputParamTable()
						.AddNew()
						.SetValue("@ObjectName", "RecursiveCallExecuteAsync")
						.SetValue("@IDs", "0")
						.Update());
				}
				return $"IActionManager.ExecuteAsync test loop count = {loopCount}, total times = {(DateTime.Now - start).TotalMilliseconds}ms";
			}
			return "";
		}
	}
}
