using S031.MetaStack.Common;
using S031.MetaStack.Core.Actions;
using S031.MetaStack.Core.App;
using S031.MetaStack.Data;
using S031.MetaStack.Actions;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using S031.MetaStack.Security;
using System.Threading;

namespace S031.MetaStack.Services
{
	internal class MessagePipeline : IDisposable
	{
		private readonly IServiceProvider _services;
		private readonly IActionManager _actionManager;
		private readonly ILoginProvider _loginProvider;
		CancellationToken _token;

		public MessagePipeline(IServiceProvider services)
		{
			_services = services;
			_actionManager = services.GetRequiredService<IActionManager>();
			_loginProvider = services.GetRequiredService<ILoginProvider>();
		}


		public void Dispose()
		{
		}
		
		public async Task<DataPackage> ProcessMessageAsync(DataPackage message, CancellationToken token)
		{
			message.NullTest(nameof(message));
			if (!message.Headers.TryGetValue("ActionID", out object actionID) ||
				actionID.ToString().IsEmpty())
				throw new MessagePipelineException("The title of the message does not contain ActionID");
			else if (!message.Headers.TryGetValue("UserName", out object UserID) ||
				UserID.ToString().IsEmpty())
				throw new MessagePipelineException("The title of the message does not contain user name");
			
			
			_token = token;
			try
			{
				ActionInfo ai = await BuildContext(message);
				if (ai.AsyncMode)
					return await _actionManager.ExecuteAsync(ai, message);
				else
					return await Task.Run(() => _actionManager.Execute(ai, message));
			}
			catch (Exception ex)
			{
				return DataPackage.CreateErrorPackage(ex);
			}
		}

		private async Task<ActionInfo> BuildContext(DataPackage message)
		{
			var config = _services.GetRequiredService<IConfiguration>();
			string actionID = (string)message.Headers["ActionID"];
			string userName = (string)message.Headers["UserName"];
			string sessionID = (string)message.Headers["SessionID"];
			string encryptedKey = (string)message.Headers["EncryptedKey"];
			if (!message.Headers.TryGetValue("ConnectionName", out object connectionName))
				connectionName = config["appSettings:defaultConnection"]; ;

			ActionInfo ai = await _actionManager.GetActionInfoAsync(actionID);
			ActionContext ctx = new ActionContext(_services)
			{
				CancellationToken = _token,
				UserName = userName,
				SessionID = sessionID,
				ConnectionName = (string)connectionName
			};

			if (ai.AuthenticationRequired)
			{
				var ui = await _loginProvider.LogonAsync(userName, sessionID, encryptedKey);
				ctx.Principal = ui;
			}
			else
				ctx.Principal = _guest;

			ai.SetContext(ctx);
			return ai;
		}
	}

	internal class MessagePipelineException:Exception
	{
		public MessagePipelineException(string message) : base(message) { }
	}
}
