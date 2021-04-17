using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using S031.MetaStack.Common;
using S031.MetaStack.Data;
using S031.MetaStack.Actions;
using S031.MetaStack.Security;
using S031.MetaStack.Integral.Security;

namespace S031.MetaStack.Security
{
	/// <summary>
	/// !!!
	/// Добавить функционал создания администратора:
	/// Добавить функционал создания администратора безопасности:
	/// если в роли security_admin_role пользователей нет то администратор может добавить пользователя в эту роль
	/// затем все манипуляции с ролями может делать только администратор безопасности
	/// одновременно пользователь не может быть членом ролей Sys.Admin и Sys.SecurityAdmin
	/// аудит таблиц Пользователи, Роли, Права доступа всегда включен
	/// </summary>
	public class BasicAuthorizationProvider : IAuthorizationProvider
	{
		public const string admin_role = "Sys.Admin";
		public const string security_admin_role = "Sys.SecurityAdmin";

		/// <summary>
		/// Name of object in format [SchemaName].[ObjectName] (as dbo.Accounts)
		/// </summary>
		public virtual bool HasPermission(ActionInfo actionInfo, string objectName)
		{
			var user = actionInfo.GetContext().Principal;
			return IsAdmin(user)
				? true
				: user.UserPermissions.Any(p => p.ActionID.Equals(actionInfo.ActionID, StringComparison.OrdinalIgnoreCase)
					&& (actionInfo.IsStatic
						|| objectName.Equals(p.ObjectName, StringComparison.OrdinalIgnoreCase)));
		}

		public virtual async Task<bool> HasPermissionAsync(ActionInfo actionInfo, string objectName)
			=> await Task.Run(() => HasPermission(actionInfo, objectName));

		public virtual bool IsAdmin(UserInfo user)
			=> UserInRore(user, admin_role);

		public virtual async Task<bool> IsAdminAsync(UserInfo user)
			=> await UserInRoreAsync(user, admin_role);

		public virtual bool IsSecurityAdmin(UserInfo user)
			=> UserInRore(user, security_admin_role);

		public virtual async Task<bool> IsSecurityAdminAsync(UserInfo user)
			=> await UserInRoreAsync(user, security_admin_role);

		public virtual bool UserInRore(UserInfo user, string roleName)
			=> user.Roles.Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase));

		public virtual async Task<bool> UserInRoreAsync(UserInfo user, string roleName)
			=> await Task.Run(() => user.Roles.Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase)));
	}
}
