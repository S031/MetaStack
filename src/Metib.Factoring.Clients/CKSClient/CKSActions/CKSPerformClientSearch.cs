using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Actions;
using S031.MetaStack.Common;
using S031.MetaStack.Data;
using S031.MetaStack.Integral.Settings;
using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
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
		private static RpcClient _rpcClient;
		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
			=> throw new NotImplementedException();

		public async Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			IServiceProvider services = ai.GetContext().Services;
			ILogger logger = services.GetRequiredService<ILogger>();
			ISettingsProvider<VocabularySettingsProvider> settings
				= services.GetRequiredService<ISettingsProvider<VocabularySettingsProvider>>();

			var rabbitConnectorOptions = (JsonObject)(await settings.GetSettings("CKSServiceSettings.RabbitConnectorOptions"));

			dp.GoDataTop();
			List<object> p = new List<object>();
			for (; dp.Read();)
			{
				p.Add(dp[0]);
				p.Add(dp[1]);
			}

			StringBuilder b = new StringBuilder();
			if (_rpcClient == null)
				_rpcClient = new RpcClient(rabbitConnectorOptions, logger);

			if (_rpcClient.Status == RpcClientStatusCodes.None)
				_rpcClient.Login(
					(string)rabbitConnectorOptions["UserLogin"],
					(string)rabbitConnectorOptions["UserPassword"]);

			if (_rpcClient.Status == RpcClientStatusCodes.OK)
			{
				var list = _rpcClient.Read(p.ToArray());
				b.AppendLine("<clients>");
				for (int i = 0; i < list.Length; i++)
					b.Append(list[i].ToString());
				b.AppendLine("</clients>");
			}
			else //!!! Replace with normal exception
				throw new InvalidOperationException("Error access to RabbitMQ service");

			return new DataPackage("@Result.String.2048")
				.AddNew()
				.SetValue("@Result", b.ToString())
				.Update();
		}
	}
}