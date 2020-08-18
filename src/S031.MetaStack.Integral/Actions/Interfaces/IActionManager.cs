using S031.MetaStack.Actions;
using S031.MetaStack.Data;
using System.Threading.Tasks;

namespace S031.MetaStack.Actions
{
	public interface IActionManager
	{
		DataPackage Execute(string actionID, DataPackage inParamStor);
		Task<DataPackage> ExecuteAsync(string actionID, DataPackage inParamStor);
		ActionInfo GetActionInfo(string actionID);
		Task<ActionInfo> GetActionInfoAsync(string actionID);
	}
}