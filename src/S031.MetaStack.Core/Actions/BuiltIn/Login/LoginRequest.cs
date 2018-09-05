using System;
using System.Collections.Generic;
using System.Text;
using S031.MetaStack.Core;
using S031.MetaStack.Core.Data;
using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Core.Security;

namespace S031.MetaStack.Core.Actions
{
	internal class LoginRequest : IAppEvaluator
	{
		public DataPackage Invoke(DataPackage dp)
		{
			string userName = (string)dp["UserName"];
			string publicKey = (string)dp["PublicKey"];
			var key = App.ApplicationContext.GetServices().GetService<ILoginFactory>().LoginRequest(userName, publicKey);
			return new DataPackage("PublicKey", key);
		}
	}
}
