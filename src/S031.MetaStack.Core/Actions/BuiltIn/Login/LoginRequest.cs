using System;
using S031.MetaStack.Core.Data;
using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Core.Security;
using System.Threading.Tasks;

namespace S031.MetaStack.Core.Actions
{
	internal class LoginRequest : IAppEvaluator
	{
		public DataPackage Invoke(DataPackage dp)
		{
			string userName = (string)dp["UserName"];
			string publicKey = (string)dp["PublicKey"];
			var key = App.ApplicationContext
				.GetServices()
				.GetService<ILoginFactory>()
				.LoginRequest(userName, publicKey);
			return new DataPackage("PublicKey", key);
		}

		public Task<DataPackage> InvokeAsync(DataPackage dp)
		{
			throw new InvalidOperationException("This action can not be executed in asynchronous mode");
		}
	}
}
