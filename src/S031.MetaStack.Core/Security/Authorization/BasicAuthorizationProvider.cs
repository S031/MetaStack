using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S031.MetaStack.Core.Actions;
using S031.MetaStack.Core.Data;

namespace S031.MetaStack.Core.Security
{
	public class BasicAuthorizationProvider : IAuthorizationProvider
	{
		public const string admin_role = "Sys.Admin";
		public const string security_admin_role = "Sys.SecurityAdmin";

		/// <summary>
		/// !!! Create global cache with update or clear possibilities
		/// </summary>
		private static readonly object _obj4Lock = new object();
		private static readonly Dictionary<string, List<string>> _user2RolesCache
			= new Dictionary<string, List<string>>(StringComparer.CurrentCultureIgnoreCase);

		private readonly MdbContext _ctx;
		public BasicAuthorizationProvider(MdbContext sysCatContext)
		{
			_ctx = sysCatContext;
		}
		public bool HasPermission(ActionInfo actionInfo, string objectName)
		{
			//!!!
			return true;
		}

		public async Task<bool> HasPermissionAsync(ActionInfo actionInfo, string objectName)
		{
			if (await IsAdminAsync(actionInfo.GetContext().UserName))
				return true;
			return false;
		}

		public bool IsAdmin(string userName)
			=> UserInRore(userName, admin_role);

		public async Task<bool> IsAdminAsync(string userName)
			=> await UserInRoreAsync(userName, admin_role);

		public bool IsSecurityAdmin(string userName)
			=> UserInRore(userName, security_admin_role);

		public async Task<bool> IsSecurityAdminAsync(string userName)
			=> await UserInRoreAsync(userName, security_admin_role);

		public bool UserInRore(string userName, string roleName)
			=> UserInRoreAsync(userName, roleName)
			.GetAwaiter()
			.GetResult();

		public async Task<bool> UserInRoreAsync(string userName, string roleName)
		{

			if (!_user2RolesCache.ContainsKey(userName))
			{
				using (var dr = await _ctx.GetReaderAsync($@"
				select r.RoleName
				from Users u
				inner join Users2Roles ur on u.ID = ur.UserID
				inner join Roles r on r.ID = ur.RoleID
				where u.UserName LIKE '{userName}'"))
				{
					if (!_user2RolesCache.ContainsKey(userName))
					{
						lock (_obj4Lock)
						{
							List<string> roles = new List<string>();
							_user2RolesCache.Add(userName, roles);
							for (; dr.Read();)
								roles.Add((string)dr["RoleName"]);
						}
					}
				}
			}
			return _user2RolesCache[userName]
				.Any(r => r.Equals(roleName, StringComparison.CurrentCultureIgnoreCase));
		}
	}
}
