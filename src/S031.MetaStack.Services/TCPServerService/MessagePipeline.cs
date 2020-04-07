using S031.MetaStack.Common;
using S031.MetaStack.Core.Actions;
using S031.MetaStack.Core.App;
using S031.MetaStack.Data;
using S031.MetaStack.Actions;
using System;
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

			using (ActionManager am = ApplicationContext.GetActionManager())
			{
				string actionID = (string)_message.Headers["ActionID"];
				ActionInfo ai = am.GetActionInfo(actionID);
				if (ai.AuthenticationRequired)
				{
					actionContext.UserName = (string)_message.Headers["UserName"];
					actionContext.SessionID = (string)_message.Headers["SessionID"];
					ApplicationContext
						.GetLoginProvider()
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
