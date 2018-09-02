using S031.MetaStack.Common;
using S031.MetaStack.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Services
{
	internal class MessagePipeline
	{
		readonly DataPackage _message = null;
		public MessagePipeline(DataPackage message)
		{
			message.NullTest(nameof(message));
			if (!message.Headers.TryGetValue("ActionID", out object actionID) ||
				actionID.ToString().IsEmpty())
				throw new MessagePipelineException("The title of the message does not contain ActionID");
			else if (!message.Headers.TryGetValue("UseID", out object UserID) ||
				actionID.ToString().IsEmpty())
				throw new MessagePipelineException("The title of the message does not contain user name");


		}

		public DataPackage Message => _message;

	}

	internal class MessagePipelineException:Exception
	{
		public MessagePipelineException(string message) : base(message) { }
	}
}
