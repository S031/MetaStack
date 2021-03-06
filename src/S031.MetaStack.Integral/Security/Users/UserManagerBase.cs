﻿using S031.MetaStack.Common;
using S031.MetaStack.Data;
using S031.MetaStack.Json;
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
		private const string _sql_get_user_info_2 = @"
			select u.ID
					,u.StructuralUnitID
					,u.AccessLevelID
					,u.UserName
					,COALESCE(u.DomainName, '') as DomainName
					,COALESCE(u.PersonID, 0) as PersonID
					,COALESCE(u.Name, '') as Name
					,COALESCE(u.JData, '') as JData
			from Users u
			where u.UserName LIKE '{0}';
			select 
				Upper(r.RoleName) as RoleName
			from Users u
			inner join Users2Roles ur on u.ID = ur.UserID
			inner join Roles r on r.ID = ur.RoleID
			where u.UserName LIKE '{0}';
			select 
				a.ActionName as ActionID
				,COALESCE(s.SchemaName + '.' + s.ObjectName, '') as ObjectName
				,r.RoleName as RoleID
			    ,p.IsGranted
			from Actions a
			inner join Permissions p on a.ID = p.ActionID
			inner join Roles r on r.ID = p.RoleID
			left join Users2Roles ur on r.ID = ur.RoleID
			left join Users u on u.ID = ur.UserID 
			left join V_SysSchemas s on s.ID = p.SchemaID
			where u.UserName LIKE '{0}' or r.RoleName like 'Sys.Everyone'";

		protected MdbContext mdb;

		/// <summary>
		/// Null - if login not found no catalog
		/// </summary>
		/// <param name="login"></param>
		/// <returns></returns>
		public virtual async Task<UserInfo> GetUserInfoAsync(string login)
		{
			var dr = await mdb.GetReadersAsync(_sql_get_user_info_2.ToFormat(login));
			UserInfo ui = null;
			if (dr[0].Read()) 
			{
				ui = JsonSerializer.DeserializeObject<UserInfo>((string)dr[0]["JData"]);
				ui.StructuralUnitID = (int)dr[0]["StructuralUnitID"];
				ui.AccessLevelID = (int)dr[0]["AccessLevelID"];
				ui.PersonID = dr[0]["PersonID"].CastAs<int>();
				ui.DomainName = (string)dr[0]["DomainName"];
				ui.Name = (string)dr[0]["Name"];
				ui.Roles.Clear();
				ui.UserPermissions.Clear();

				if (dr[1].Read())
					ui.Roles.Add((string)dr[1]["RoleName"]);

				for (; dr[2].Read();)
					ui.UserPermissions.Add(new Permission()
					{
						ActionID = (string)dr[2]["ActionID"],
						ObjectName = (string)dr[2]["ObjectName"], 
						RoleID = (string)dr[2]["RoleID"], 
						IsGranted = (bool)dr[2]["IsGranted"]
					});
			}
			dr[0].Dispose();
			dr[1].Dispose();
			dr[2].Dispose();
			return ui;
		}
		public virtual UserInfo GetUserInfo(string login)
			=> GetUserInfoAsync(login)
			.GetAwaiter()
			.GetResult();
	}
}
