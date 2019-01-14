using System;
using System.Collections.Generic;
using System.Text;
using S031.MetaStack.Core.Actions;
using S031.MetaStack.Core.Data;

namespace S031.MetaStack.Core.Security
{
	public class BasicAuthorizationProvider : IAuthorizationProvider
	{
		public const string admin_role = "Sys.Admin";
		public const string security_admin_role = "Sys.SecurityAdmin";

		private readonly MdbContext _ctx;
		public BasicAuthorizationProvider(MdbContext sysCatContext)
		{
			_ctx = sysCatContext;
		}
		public bool HasPermission(ActionInfo actionInfo, string objectName)
		{
			throw new NotImplementedException();
		}

		public bool IsAdmin(string userName) 
			=> UserInRore(userName, admin_role);

		public bool IsSecurityAdmin(string userName)
			=> UserInRore(userName, security_admin_role);

		public bool UserInRore(string userName, string roleName)
		{
			return _ctx.Execute<int>($@"select 1 from Roles
				inner join Users on Roles.RoleName = '{roleName}' and users.UserName = '{userName}'") == 1;
		}
	}
}
