using System.Threading.Tasks;

namespace S031.MetaStack.Core.Actions
{
	public interface IAppEvaluator
	{
		Task<Data.DataPackage> InvokeAsync(ActionInfo ai, Data.DataPackage dp);
		Data.DataPackage Invoke(ActionInfo ai, Data.DataPackage dp);
	}
}
