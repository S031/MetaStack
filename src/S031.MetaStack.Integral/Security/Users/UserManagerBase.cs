using S031.MetaStack.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Integral.Security
{
	public class UserManagerBase: IUserManager
	{
		private const string sql_get_user_info = @"
			SELECT u.ID,
				   u.StructuralUnitID,
				   u.AccessLevelID,
				   u.UserName,
				   COALESCE(u.DomainName, '') as DomainName,
				   COALESCE(u.PersonID, 0) as PersonID,
				   COALESCE(u.Name, '') as Name,
				   Upper(COALESCE(r.RoleName, 'SYS.PUBLIC')) as RoleName
			FROM Users u
			left join Users2Roles ur on u.ID = ur.UserID
			left join Roles r on r.ID = ur.RoleID
			where u.UserName LIKE '{0}'";
		
		private const string sql_get_user_info_2 = @"
			select u.ID,
				   u.StructuralUnitID,
				   u.AccessLevelID,
				   u.UserName,
				   COALESCE(u.DomainName, '') as DomainName,
				   COALESCE(u.PersonID, 0) as PersonID,
				   COALESCE(u.Name, '') as Name			
			from Users u
			where u.UserName LIKE '{}';
			select 
				Upper(r.RoleName) as RoleName
			from Users u
			inner join Users2Roles ur on u.ID = ur.UserID
			inner join Roles r on r.ID = ur.RoleID
			where u.UserName LIKE 'hq\svostrikov'";

		protected MdbContext mdb;

		async Task<UserInfo> GetUserInfoAsync(string login)
		{

		}
		UserInfo GetUserInfo(string login) 
		{ 
			throw new NotImplementedException();
		}
	}
}
