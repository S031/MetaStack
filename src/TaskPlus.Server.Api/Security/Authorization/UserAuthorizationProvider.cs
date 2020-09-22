using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Actions;
using S031.MetaStack.Common;
using S031.MetaStack.Data;
using S031.MetaStack.Integral.Security;
using S031.MetaStack.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskPlus.Server.Api.Properties;
using TaskPlus.Server.Data;

namespace TaskPlus.Server.Security
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
	public class UserAuthorizationProvider : IAuthorizationProvider
	{
		public const string admin_role = "Sys.Admin";
		public const string security_admin_role = "Sys.SecurityAdmin";

		private readonly IServiceProvider _services;
		//private readonly IConfiguration _config;
		//private readonly ILogger _logger;
		private readonly MdbContext _mdb;

		public UserAuthorizationProvider(IServiceProvider services)
		{
			_services = services;
			//_config = _services.GetRequiredService<IConfiguration>();
			//_logger = _services.GetRequiredService<ILogger>();
			_mdb = _services
				.GetRequiredService<IMdbContextFactory>()
				.GetContext(Strings.SysCatConnection);
		}
		
		/// <summary>
		/// Name of object in format [SchemaName].[ObjectName] (as dbo.Accounts)
		/// </summary>
		public bool HasPermission(ActionInfo actionInfo, string objectName)
		{
			var user = actionInfo.GetContext().Principal;
			return IsAdmin(user)
				? true
				: user.UserPermissions.Any(p=>p.ActionID.Equals(actionInfo.ActionID, StringComparison.OrdinalIgnoreCase)
					&& (actionInfo.IsStatic 
						|| objectName.Equals(p.ObjectName, StringComparison.OrdinalIgnoreCase)));
		}

		public async Task<bool> HasPermissionAsync(ActionInfo actionInfo, string objectName)
			=> await Task.Run(() => HasPermission(actionInfo, objectName));

		public bool IsAdmin(UserInfo user) 
			=> UserInRore(user, admin_role);

		public async Task<bool> IsAdminAsync(UserInfo user)
			=> await UserInRoreAsync(user, admin_role);

		public bool IsSecurityAdmin(UserInfo user)
			=> UserInRore(user, security_admin_role);

		public async Task<bool> IsSecurityAdminAsync(UserInfo user)
			=> await UserInRoreAsync(user, security_admin_role);

		public bool UserInRore(UserInfo user, string roleName)
			=> user.Roles.Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase));

		public async Task<bool> UserInRoreAsync(UserInfo user, string roleName)
			=> await Task.Run(() => user.Roles.Any(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase)));
	}
}
