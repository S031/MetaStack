using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core;
using S031.MetaStack.Core.App;
using S031.MetaStack.Data;
using System.Data;
using System.Data.Common;
using Xunit;

namespace MetaStack.Test.Data
{
	public class DBProviderFactoryTest
	{
		public DBProviderFactoryTest()
		{
			MetaStack.Test.Program.GetServices();
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}
		[Fact]
		public void GetFactoryTest()
		{
			using (FileLog l = new FileLog("DBProviderFactoryTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				l.Debug("SpeedTest 1 Start ");
				DbProviderFactory f = null;
				int i = 0;
				for (i = 0; i < 1000000; i++)
				{
					//f = ObjectFactories.GetFactory<DbProviderFactory>("System.Data.SqlClient");
					f = MdbContext.GetFactory("System.Data.SqlClient");
				}
				l.Debug($"SpeedTest 1 Finish {i} count result {f}");
				
				l.Debug("SpeedTest 2 Start ");
				f = null;
				for (i = 0; i < 1000000; i++)
				{
					f = MdbContext.GetFactory("System.Data.SQLite");
				}
				l.Debug($"SpeedTest 2 Finish {i} count result {f}");

				//ApplicationContext.Services.AddFromAssembly<DbProviderFactory>(ServiceLifetime.Singleton,
				//	(s, t) => t.CreateInstance2<DbProviderFactory>());
				//l.Debug("GetFactoryProviderNames:");
				//foreach (string s in ObjectFactories.GetFactoryNames<DbProviderFactory>())
				//{
				//	l.Debug(s);
				//}
				//Retrieve the installed providers and factories.
				DataTable table = System.Data.Common.DbProviderFactories.GetFactoryClasses();

				// Display each row and column value.
				foreach (DataRow row in table.Rows)
				{
					foreach (DataColumn column in table.Columns)
					{
						l.Debug(row[column]);
					}
				}
			}
		}
	}
}
