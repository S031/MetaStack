using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using S031.MetaStack.Core.Actions;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Data;
using S031.MetaStack.Core.Security;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Services
{
	internal class MessagePipeline : IDisposable
	{
		private static readonly object obj4Lock = new object();
		readonly DataPackage _message = null;
		DataPackage _result = null;
		public MessagePipeline(DataPackage message)
		{
			message.NullTest(nameof(message));
			if (!message.Headers.TryGetValue("ActionID", out object actionID) ||
				actionID.ToString().IsEmpty())
				throw new MessagePipelineException("The title of the message does not contain ActionID");
			else if (!message.Headers.TryGetValue("UserName", out object UserID) ||
				actionID.ToString().IsEmpty())
				throw new MessagePipelineException("The title of the message does not contain user name");
			_message = message;
		}

		public DataPackage InputMessage => _message;
		public DataPackage ResultMessage => _result;

		public void Dispose()
		{
		}
		public async Task ProcessMessageAsync()
		{
			var config = ApplicationContext.GetConfiguration();
			if (!_message.Headers.TryGetValue("ConnectionName", out object connectionName))
				connectionName = config["appSettings:defaultConnection"]; ;
			ActionContext actionContext = new ActionContext() { ConnectionName = (string)connectionName };

			var cs = config.GetSection($"connectionStrings:{config["appSettings:SysCatConnection"]}").Get<ConnectInfo>();
			using (MdbContext mdb = new MdbContext(cs))
			using (ActionManager am = new ActionManager(mdb) { Logger = ApplicationContext.GetLogger() })
			{
				string actionID = (string)_message.Headers["ActionID"];
				ActionInfo ai = am.GetActionInfo(actionID);
				if (ai.AuthenticationRequired)
				{
					actionContext.UserName = (string)_message.Headers["UserName"];
					actionContext.SessionID = (string)_message.Headers["SessionID"];
					ApplicationContext
						.GetLoginFactory()
						.Logon(
						(string)_message.Headers["UserName"],
						(string)_message.Headers["SessionID"],
						(string)_message.Headers["EncryptedKey"]);
					//set thread principal
				}
				ai.SetContext(actionContext);
				if (ai.AsyncMode)
					_result = await am.ExecuteAsync(actionID, _message);
				else
					_result = am.Execute(actionID, _message);
			}
		}
	}

	internal class MessagePipelineException:Exception
	{
		public MessagePipelineException(string message) : base(message) { }
	}
}
