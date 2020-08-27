using S031.MetaStack.Actions;
using S031.MetaStack.Integral.Security;
using S031.MetaStack.Security;
using System;
using System.Threading.Tasks;

namespace TaskPlus.Server.Security
{
	public class UserAuthorizationProvider : IAuthorizationProvider
	{
		public bool HasPermission(ActionInfo actionInfo, UserInfo user)
		{
			throw new NotImplementedException();
		}

		public Task<bool> HasPermissionAsync(ActionInfo actionInfo, UserInfo user)
		{
			throw new NotImplementedException();
		}

		public bool IsAdmin(UserInfo user)
		{
			throw new NotImplementedException();
		}

		public Task<bool> IsAdminAsync(UserInfo user)
		{
			throw new NotImplementedException();
		}

		public bool IsSecurityAdmin(UserInfo user)
		{
			throw new NotImplementedException();
		}

		public Task<bool> IsSecurityAdminAsync(UserInfo user)
		{
			throw new NotImplementedException();
		}

		public bool UserInRore(UserInfo user, string roleName)
		{
			throw new NotImplementedException();
		}

		public Task<bool> UserInRoreAsync(UserInfo user, string roleName)
		{
			throw new NotImplementedException();
		}
	}
}
