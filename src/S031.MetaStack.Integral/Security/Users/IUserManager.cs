using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Integral.Security
{
	public interface IUserManager
	{
		Task<UserInfo> GetUserInfoAsync(string login);
		UserInfo GetUserInfo(string login);
	}
}
