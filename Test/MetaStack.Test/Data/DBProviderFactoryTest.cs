﻿using Xunit;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.Data;
using System.Data.Common;
using System.Data;
using System.Linq;
using S031.MetaStack.Core;
using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Common;
using S031.MetaStack.Core.App;

namespace MetaStack.Test.Data
{
	public class DBProviderFactoryTest
	{
		public DBProviderFactoryTest()
		{
			MetaStack.Test.Program.ConfigureTests();
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
		}
		[Fact]
		void GetFactoryTest()
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
				l.Debug($"SpeedTest 1 Finish {i} count result {f}");
				ApplicationContext.Services.AddFromAssembly<DbProviderFactory>(ServiceLifetime.Singleton,
					(s, t) => t.CreateInstance2<DbProviderFactory>());
				l.Debug("GetFactoryProviderNames:");
				foreach (string s in ObjectFactories.GetFactoryNames<DbProviderFactory>())
					l.Debug(s);
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
