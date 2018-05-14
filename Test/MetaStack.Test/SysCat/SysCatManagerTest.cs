
using Microsoft.Extensions.Logging;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.Data;
using S031.MetaStack.Core.Logging;
using S031.MetaStack.Core.SysCat;
using System;
using System.Data.Common;
using System.Threading.Tasks;
using Xunit;

namespace MetaStack.Test.SysCat
{
	public class SysCatManagerTest
	{
		string _cn;
		public SysCatManagerTest()
		{
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
			_cn = (new MetaStack.Test.Data.MdbContextTest()).connectionString;
			DbConnectionStringBuilder sb = new DbConnectionStringBuilder();
			sb.ConnectionString = _cn;
			sb["Initial Catalog"] = "MetaStack";
			_cn = sb.ToString();
		}
		//[Fact]
		//async Task createDbAsyncTest()
		//{

		//	using (FileLogger l = new FileLogger("SysCatManagerTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
		//	using (var ctx = await MdbContext.CreateMdbContextAsync(_cn, l))
		//	{
		//		await new SysCatManager(ctx).CreateDbSchemaAsync();
		//	}
		//}
		//[Fact]
		//async Task dropDbAsyncTest()
		//{

		//	using (FileLogger l = new FileLogger("SysCatManagerTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
		//	using (var ctx = await MdbContext.CreateMdbContextAsync(_cn, l))
		//	{
		//		await new SysCatManager(ctx).DropDbSchemaAsync();
		//	}
		//}
		//[Fact]
		//async Task syncSchemaAsyncTest()
		//{

		//	using (FileLogger l = new FileLogger("SysCatManagerTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
		//	using (var ctx = await MdbContext.CreateMdbContextAsync(_cn, l))
		//	{
		//		await new SysCatManager(ctx).SyncSchemaAsync("SysSequence");
		//	}
		//}
	}
}
