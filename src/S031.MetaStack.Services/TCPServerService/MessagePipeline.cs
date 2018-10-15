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
		static readonly string connection_name = Dns.GetHostName() == "SERGEY-WRK" ? "Test" : "BankLocal";
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
			if (!_message.Headers.TryGetValue("ConnectionName", out object connectionName))
				connectionName = connection_name;

			var serviceProvider = ApplicationContext.GetServices();
			var configuration = serviceProvider.GetService<IConfiguration>();
			var cs = configuration.GetSection($"connectionStrings:{connectionName}").Get<ConnectInfo>();
			using (MdbContext mdb = await MdbContext.CreateMdbContextAsync(cs))
			using (ActionManager am = new ActionManager(mdb))
			{
				string actionID = (string)_message.Headers["ActionID"];
				ActionInfo ai = am.GetActionInfo(actionID);
				if (ai.AuthenticationRequired)
				{
					var loginFactory = serviceProvider.GetService<ILoginFactory>();
					loginFactory.Logon(
						(string)_message.Headers["UserName"],
						(string)_message.Headers["SessionID"],
						(string)_message.Headers["EncryptedKey"]);
					//set thread principal
				}
				//System.IO.File.WriteAllText(@"c:\source\a123.txt", _message.ToString());
				//serviceProvider.GetService<ILogger>().LogTrace();
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
