using S031.MetaStack.Actions;
using System.Threading.Tasks;

namespace S031.MetaStack.Security
{
	public interface IAuthorizationProvider
	{
		bool IsAdmin(string userName);
		Task<bool> IsAdminAsync(string userName);
		bool IsSecurityAdmin(string userName);
		Task<bool> IsSecurityAdminAsync(string userName);
		bool HasPermission(ActionInfo actionInfo, string objectName);
		Task<bool> HasPermissionAsync(ActionInfo actionInfo, string objectName);
		bool UserInRore(string userName, string roleName);
		Task<bool> UserInRoreAsync(string userName, string roleName);
	}
}
