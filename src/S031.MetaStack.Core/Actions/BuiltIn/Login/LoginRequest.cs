using System;
using S031.MetaStack.Core.Data;
using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Core.Security;
using System.Threading.Tasks;

namespace S031.MetaStack.Core.Actions
{
	internal class LoginRequest : IAppEvaluator
	{
		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			string userName = (string)dp["UserName"];
			string publicKey = (string)dp["PublicKey"];
			var key = App.ApplicationContext
				.GetServices()
				.GetService<ILoginFactory>()
				.LoginRequest(userName, publicKey);

			var result = ai.GetOutputParamTable();
			result.AddNew();
			result["LoginInfo"] = key;
			result.Update();
			return result;
		}

		public Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			throw new InvalidOperationException("This action can not be executed in asynchronous mode");
		}
	}
	internal class Logon : IAppEvaluator
	{
		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			string userName = (string)dp["UserName"];
			string sessionID = (string)dp["SessionID"];
			string encryptedKey = (string)dp["EncryptedKey"];
			var key = App.ApplicationContext
				.GetServices()
				.GetService<ILoginFactory>()
				.Logon(userName, sessionID, encryptedKey);

			var result = ai.GetOutputParamTable();
			result.AddNew();
			result["Ticket"] = key;
			result.Update();
			return result;
		}

		public Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			throw new InvalidOperationException("This action can not be executed in asynchronous mode");
		}
	}
}
