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
		private readonly IConfiguration _config;
		private readonly ILogger _logger;
		private readonly MdbContext _mdb;

		/// <summary>
		/// !!! Create global cache with update or clear possibilities
		/// </summary>
		private static readonly MapTable<string, List<string>> _user2RolesCache
			= new MapTable<string, List<string>>(StringComparer.Ordinal);

		public UserAuthorizationProvider(IServiceProvider services)
		{
			_services = services;
			_config = services.GetRequiredService<IConfiguration>();
			_logger = services.GetRequiredService<ILogger>();
			_mdb = services
				.GetRequiredService<IMdbContextFactory>()
				.GetContext(Strings.SysCatConnection);
		}
		public bool HasPermission(ActionInfo actionInfo, string objectName)
			=> HasPermissionAsync(actionInfo, objectName)
			.GetAwaiter()
			.GetResult();

		public async Task<bool> HasPermissionAsync(ActionInfo actionInfo, string objectName)
		{
			if (await IsAdminAsync(actionInfo.GetContext().Principal))
				return true;
			/*!!!
             * select * from users u
             * inner join User2Roles r on u.id = r.UserID
             * inner join permissions p on p.RoleID = r.roleId
             * inner join actions a on a.id = p.ActionId
             * where u.UserName = '{UserName}' and objectName = {if static action any else objectName}
             * and a.actionname = actionInfo.actionId
             */
			return false;
		}

		public bool IsAdmin(UserInfo user) 
			=> UserInRore(user, admin_role);

		public async Task<bool> IsAdminAsync(UserInfo user)
			=> await UserInRoreAsync(user, admin_role);

		public bool IsSecurityAdmin(UserInfo user)
			=> UserInRore(user, security_admin_role);

		public async Task<bool> IsSecurityAdminAsync(UserInfo user)
			=> await UserInRoreAsync(user, security_admin_role);

		public bool UserInRore(UserInfo user, string roleName)
			=> UserInRoreAsync(user, roleName)
			.GetAwaiter()
			.GetResult();

		public async Task<bool> UserInRoreAsync(UserInfo user, string roleName)
		{
			string key = user.UserName.ToUpper();
			string role = roleName.ToUpper();

			if (!_user2RolesCache.ContainsKey(key))
				await SetUser2RolesCache(key);
			return _user2RolesCache[key]
				.Any(r => r.Equals(role, StringComparison.Ordinal));
		}

		private async Task SetUser2RolesCache(string key)
		{
			string sql = $@"
				select Upper(r.RoleName) as RoleName
				from Users u
				inner join Users2Roles ur on u.ID = ur.UserID
				inner join Roles r on r.ID = ur.RoleID
				where u.UserName LIKE '{key}'";

			using (var dr = await _mdb.GetReaderAsync(sql))
			{
				if (!_user2RolesCache.ContainsKey(key))
				{
					List<string> roles = new List<string>();
					for (; dr.Read();)
						roles.Add((string)dr["RoleName"]);
					_user2RolesCache.TryAdd(key, roles);
				}
			}
		}
	}
}
