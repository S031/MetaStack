using S031.MetaStack.Actions;
using S031.MetaStack.Data;
using System;
using System.Threading.Tasks;
using System.Xml;

namespace Metib.Factoring.Clients
{
	public class CKSTest : IAppEvaluator
	{
		public DataPackage Invoke(ActionInfo ai, DataPackage dp)
		{
			throw new NotImplementedException();
		}

		public async Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp)
		{
			var ctx = ai.GetContext();
			var result = ai.GetOutputParamTable()
				.AddNew()
				.SetValue("@Result", "OK")
				.Update();
			return await Task.Run<DataPackage>(() => result);
		}
	}
}
