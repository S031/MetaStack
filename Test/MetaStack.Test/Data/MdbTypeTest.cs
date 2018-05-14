using Xunit;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core;
using System.Collections.Generic;
using System;
using S031.MetaStack.Core.Data;

namespace MetaStack.Test.Data
{
	public class MdbTypeTest
	{
		public MdbTypeTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}
		[Fact]
		void speedTest()
		{
			using (FileLog l = new FileLog("MdbTypeTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				Type t = null;
				MdbTypeInfo info = new MdbTypeInfo();
				l.Debug("SpeedTest 1 Start ");
				int i = 0;
				for (i = 0; i < 1000000; i++)
				{
					t = MdbTypeMap.GetType("object", typeof(string));
				}
				l.Debug($"SpeedTest 1 Finish {i} count result {t}");
				for (i = 0; i < 1000000; i++)
				{
					t = MdbTypeMap.GetType("failed", typeof(string));
				}
				l.Debug($"SpeedTest 2 Finish {i} count result {t}");
				for (i = 0; i < 1000000; i++)
				{
					info = MdbTypeMap.GetTypeInfo(typeof(string));
				}
				l.Debug($"SpeedTest 3 Finish {i} count result {info.Name}");
			}
		}
	}
}
