using System;
using System.Threading.Tasks;
using S031.MetaStack.Core.App;
using S031.MetaStack.Data;
using S031.MetaStack.ORM.Actions;

namespace S031.MetaStack.Core.Actions
{
	internal class SysPipeRead : IAppEvaluator
	{
		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			return ai.GetOutputParamTable()
				.AddNew()
				.SetValue("Result", ApplicationContext.GetPipe().Read(ai.GetContext()))
				.Update();
		}

		public Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			throw new NotImplementedException();
		}
	}
}

