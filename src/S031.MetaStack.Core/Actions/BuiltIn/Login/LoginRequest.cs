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
			if (result == null)
				System.IO.File.WriteAllText(@"c:\source\a123.txt", "result is null");
			result["PublicKey"] = key;
			result.Update();
			System.IO.File.WriteAllText(@"c:\source\a123.txt", result.ToString());
			return result;
		}

		public Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			throw new InvalidOperationException("This action can not be executed in asynchronous mode");
		}
	}
}
