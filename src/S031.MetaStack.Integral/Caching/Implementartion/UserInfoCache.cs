using S031.MetaStack.Common;
using S031.MetaStack.Integral.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Caching
{
	public class UserInfoCache : MapTable<string, UserInfo>, IDataCache<string, UserInfo>
	{
		public static UserInfoCache Instance = new UserInfoCache();
		private UserInfoCache() { }
	}
}
