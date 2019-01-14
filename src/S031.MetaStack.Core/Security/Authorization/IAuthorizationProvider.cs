using S031.MetaStack.Core.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Core.Security
{
	public interface IAuthorizationProvider
	{
		bool IsAdmin(string userName);
		bool IsSecurityAdmin(string userName);
		bool HasPermission(ActionInfo actionInfo, string objectName);
		bool UserInRore(string userName, string roleName);
	}
}
