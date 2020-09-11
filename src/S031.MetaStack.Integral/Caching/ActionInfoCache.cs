using S031.MetaStack.Actions;
using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Caching
{
	public class ActionInfoCache : MapTable<string, ActionInfo>, IDataCache<string, ActionInfo>
	{
		public static ActionInfoCache Instance = new ActionInfoCache();
		private ActionInfoCache() { }
	}
}
