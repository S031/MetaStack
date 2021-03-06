﻿using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using S031.MetaStack.Data;
using S031.MetaStack.Actions;
using S031.MetaStack.Security;

namespace TaskPlus.Server.Actions
{
	internal class SysLoginRequest : IAppEvaluator
	{
		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
			=> InvokeAsync(ai, dp)
			.GetAwaiter()
			.GetResult();

		public async Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			ActionContext ctx = ai.GetContext();
			string userName = (string)dp["UserName"];
			string password = (string)dp["Password"];
			var key = await ctx
				.Services
				.GetService<ILoginProvider>()
				.LoginRequestAsync(userName, password);

			return  ai.GetOutputParamTable()
				.AddNew()
				.SetValue("JwtToken", key)
				.Update();
		}
	}
	internal class SysLogon : IAppEvaluator
	{
		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			ActionContext ctx = ai.GetContext();
			string userName = (string)dp["UserName"];
			string sessionID = (string)dp["SessionID"];
			string encryptedKey = (string)dp["EncryptedKey"];
			var key = ctx
				.Services
				.GetService<ILoginProvider>()
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
			ActionContext ctx = ai.GetContext();
			string userName = (string)dp.Headers["UserName"];
			string sessionID = (string)dp.Headers["SessionID"];
			string encryptedKey = (string)dp.Headers["EncryptedKey"];
			ctx
				.Services
				.GetService<ILoginProvider>()
				.Logout(userName, sessionID, encryptedKey);
			return DataPackage.CreateOKPackage();
		}

		public Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			throw new InvalidOperationException("This action can not be executed in asynchronous mode");
		}
	}
}
