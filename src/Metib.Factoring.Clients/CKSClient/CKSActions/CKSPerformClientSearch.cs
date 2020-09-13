using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Actions;
using S031.MetaStack.Common;
using S031.MetaStack.Data;
using S031.MetaStack.Integral.Settings;
using S031.MetaStack.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using TaskPlus.Server.Api.Settings;
using TaskPlus.Server.Data;

namespace Metib.Factoring.Clients.CKS
{
	public class CKSPerformClientSearch : IAppEvaluator
	{
		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
			=> throw new NotImplementedException();

		public async Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			IServiceProvider services = ai.GetContext().Services;
			ISettingsProvider<VocabularySettingsProvider> settings
				= services.GetRequiredService<ISettingsProvider<VocabularySettingsProvider>>();

			var rabbitConnectorOptions = (JsonObject)(await settings.GetSettings("CKSServiceSettings.RabbitConnectorOptions"));

			StringBuilder b = new StringBuilder();
			//string inn = (string)dp["@IDs"];
			using (var rpcClient = new RpcClient())
			{
				var loginInfo = rpcClient.Login(
					(string)rabbitConnectorOptions["UserLogin"], 
					(string)rabbitConnectorOptions["UserPassword"]);
				//var response = rpcClient.Call(message);

				b.AppendLine(loginInfo.ToString());
				var list = rpcClient.Read("inn", "7714606819");
				b.Append(list[0].ToString());
			}
			return ai.GetOutputParamTable()
				.AddNew()
				.SetValue("@Result", b.ToString())
				.Update();
		}
	}
}