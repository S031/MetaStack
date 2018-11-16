using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Common;
using S031.MetaStack.Core.App;
using S031.MetaStack.Core.Data;
using S031.MetaStack.Core.ORM;

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

