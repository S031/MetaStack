using S031.MetaStack.Data;
using S031.MetaStack.Actions;
using System.Threading.Tasks;

namespace S031.MetaStack.Core.Actions
{
	public interface IAppEvaluator
	{
		Task<DataPackage> InvokeAsync(ActionInfo ai, DataPackage dp);
		DataPackage Invoke(ActionInfo ai, DataPackage dp);
	}
}
