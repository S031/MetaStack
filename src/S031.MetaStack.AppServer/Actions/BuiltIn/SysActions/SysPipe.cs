using System;
using System.Threading.Tasks;
using S031.MetaStack.Data;
using S031.MetaStack.Actions;
using Microsoft.Extensions.DependencyInjection;

namespace S031.MetaStack.AppServer.Actions
{
	internal class SysPipeRead : IAppEvaluator
	{
		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			var ctx = ai.GetContext();
			return ai.GetOutputParamTable()
				.AddNew()
				.SetValue("Result", ctx
					.Services
					.GetRequiredService<PipeQueue>()
					.Read(ctx))
				.Update();
		}

		public Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			throw new NotImplementedException();
		}
	}
}

