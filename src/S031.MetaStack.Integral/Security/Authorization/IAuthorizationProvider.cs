using S031.MetaStack.Actions;
using S031.MetaStack.Integral.Security;
using System.Threading.Tasks;

namespace S031.MetaStack.Security
{
	public interface IAuthorizationProvider
	{
		bool IsAdmin(UserInfo user);
		Task<bool> IsAdminAsync(UserInfo user);
		bool IsSecurityAdmin(UserInfo user);
		Task<bool> IsSecurityAdminAsync(UserInfo user);
		bool HasPermission(ActionInfo actionInfo, string objectName);
		Task<bool> HasPermissionAsync(ActionInfo actionInfo, string objectName);
		bool UserInRore(UserInfo user, string roleName);
		Task<bool> UserInRoreAsync(UserInfo user, string roleName);
	}
}
