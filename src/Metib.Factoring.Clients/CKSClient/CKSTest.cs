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
		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			throw new NotImplementedException();
		}

		public async Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
			=> await Task.Run(() => InvokeInternal(ai, dp));
			//=> await InvokeInternalAsync(ai, dp);

		private DataPackage InvokeInternal(ActionInfo ai, DataPackage dp)
		{
			const string _un = "svc_factoring2";
			const string _pwd = "";
			
			StringBuilder b = new StringBuilder();
			//string inn = (string)dp["@IDs"];
			using (var rpcClient = new RpcClient())
			{
				var loginInfo = rpcClient.Login(_un, _pwd);
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
