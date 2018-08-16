using Microsoft.VisualStudio.TestTools.UnitTesting;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core;
using System.Data.Common;

namespace MetaStack.UnitTest.Data
{
	[TestClass]
	public class DBProviderFactoryTest
	{
		public DBProviderFactoryTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}
		[TestMethod]
		public void GetFactoryTest()
		{
			using (FileLog l = new FileLog("DBProviderFactoryTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				l.Debug("SpeedTest 1 Start ");
				DbProviderFactory f = null;
				int i = 0;
				for (i = 0; i < 1000000; i++)
				{
					f = ObjectFactories.GetFactory<DbProviderFactory>("System.Data.SqlClient");
				}
				l.Debug($"SpeedTest 2 Finish {i} count result {f}");
				l.Debug("GetFactoryProviderNames:");
				foreach (string s in ObjectFactories.GetFactoryNames<DbProviderFactory>())
					l.Debug(s);

			}
		}
	}
}
