using System.Threading.Tasks;

namespace S031.MetaStack.Core.Actions
{
	public interface IAppEvaluator
	{
		Task<Data.DataPackage> InvokeAsync(Data.DataPackage dp);
		Data.DataPackage Invoke(Data.DataPackage dp);
	}
}
