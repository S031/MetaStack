﻿using S031.MetaStack.Actions;
using S031.MetaStack.Common;
using S031.MetaStack.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Caching
{
	public class SettingsCache : MapTable<string, string>, IDataCache<string, string>
	{
		public static SettingsCache  Instance = new SettingsCache();
		private SettingsCache() { }
	}
}
