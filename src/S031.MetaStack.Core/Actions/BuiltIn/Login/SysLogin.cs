using System;
using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Core.Security;
using System.Threading.Tasks;
using S031.MetaStack.Data;
using S031.MetaStack.Actions;
using S031.MetaStack.Security;

namespace S031.MetaStack.Core.Actions
{
	internal class SysLoginRequest : IAppEvaluator
	{
		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			string userName = (string)dp["UserName"];
			string publicKey = (string)dp["PublicKey"];
			var key = ai.GetContext()
				.Services
				.GetRequiredService<ILoginProvider>()
				.LoginRequest(userName, publicKey);

			return ai.GetOutputParamTable()
				.AddNew()
				.SetValue("LoginInfo", key)
				.Update();
		}

		public Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			throw new InvalidOperationException("This action can not be executed in asynchronous mode");
		}
	}
	internal class SysLogon : IAppEvaluator
	{
		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			string userName = (string)dp["UserName"];
			string sessionID = (string)dp["SessionID"];
			string encryptedKey = (string)dp["EncryptedKey"];
			var key = ai.GetContext()
				.Services
				.GetRequiredService<ILoginProvider>()
				.Logon(userName, sessionID, encryptedKey);

			return ai.GetOutputParamTable()
				.AddNew()
				.SetValue("Ticket", key)
				.Update();
		}

		public Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			throw new InvalidOperationException("This action can not be executed in asynchronous mode");
		}
	}
	internal class SysLogout : IAppEvaluator
	{
		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			string userName = (string)dp.Headers["UserName"];
			string sessionID = (string)dp.Headers["SessionID"];
			string encryptedKey = (string)dp.Headers["EncryptedKey"];
			ai.GetContext()
				.Services
				.GetRequiredService<ILoginProvider>()
				.Logout(userName, sessionID, encryptedKey);
			return DataPackage.CreateOKPackage();
		}

		public Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			throw new InvalidOperationException("This action can not be executed in asynchronous mode");
		}
	}
}
