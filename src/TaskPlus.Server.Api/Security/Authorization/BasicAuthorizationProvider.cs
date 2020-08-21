using Microsoft.AspNetCore.Authentication;
using S031.MetaStack.Actions;
using S031.MetaStack.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskPlus.Server.Security
{
	public class BasicAuthorizationProvider : IAuthorizationProvider
	{
		public bool HasPermission(ActionInfo actionInfo, string objectName)
		{
			throw new NotImplementedException();
		}

		public Task<bool> HasPermissionAsync(ActionInfo actionInfo, string objectName)
		{
			throw new NotImplementedException();
		}

		public bool IsAdmin(string userName)
		{
			throw new NotImplementedException();
		}

		public Task<bool> IsAdminAsync(string userName)
		{
			throw new NotImplementedException();
		}

		public bool IsSecurityAdmin(string userName)
		{
			throw new NotImplementedException();
		}

		public Task<bool> IsSecurityAdminAsync(string userName)
		{
			throw new NotImplementedException();
		}

		public bool UserInRore(string userName, string roleName)
		{
			throw new NotImplementedException();
		}

		public Task<bool> UserInRoreAsync(string userName, string roleName)
		{
			throw new NotImplementedException();
		}
	}
}
