using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.App;
using S031.MetaStack.Data;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MetaStack.Test.Data
{
	public class MdbContextTest
	{
		private static readonly string connection_name = Dns.GetHostName() == "DESKTOP-14MM2MF" ? "Test" : "BankLocal";
		private readonly IConfiguration _configuration;
		private readonly string _cn;
		private readonly string _providerName;
		private readonly string _syscat;
		private readonly string _syscatProviderName;

		internal string providerName => _providerName;
		internal string connectionString => _cn;

		public MdbContextTest()
		{
			var services = MetaStack.Test.Program.GetServices();

			_configuration = services.GetService<IConfiguration>();
			var cs = _configuration.GetSection($"connectionStrings:{connection_name}").Get<ConnectInfo>();
			_providerName = cs.ProviderName;
			_cn = new ConnectInfo(_providerName, cs.ConnectionString).ToString();

			var s = _configuration["appSettings:SysCatConnection"];
			cs = _configuration.GetSection($"connectionStrings:{s}").Get<ConnectInfo>();
			_syscatProviderName = cs.ProviderName;
			_syscat = new ConnectInfo(_syscatProviderName, cs.ConnectionString).ToString();
			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;

		}
		[Fact]
		private void createContextTest()
		{
			using (FileLog l = new FileLog("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				Type t = null;
				l.Debug("SpeedTest 1 Start ");
				int i = 0;
				for (i = 0; i < 1000000; i++)
				{
					using (var ctx = new MdbContext(_cn))
					{
					}
				}
				l.Debug($"SpeedTest 1 Finish {i} count result {t}");
			}
		}
		[Fact]
		public void GetReaderSpeedTest()
		{
			using (FileLog l = new FileLog("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				DateTime t = DateTime.MinValue;
				//Caching pool
				int i = 0;
				for (i = 0; i < 100; i++)
				{
					using (var ctx = new MdbContext(_cn))
					{
					}
				}
				using (var ctx = new MdbContext(_cn))
				{
					l.Debug("SpeedTest 1 Start ");
					MdbContextOptions.GetOptions().CommandTimeout = 120;
					for (i = 0; i < 1000; i++)
					{
						using (var dr = ctx.GetReader("Select * From Bank..PayDocs Where Handle = @handle",
							new MdbParameter("@handle", 3999758)))
						{
							dr.Read();
						}
					}
					l.Debug($"SpeedTest 1 Finish {i} count result {i}");
					l.Debug("SpeedTest 2 Start ");
					for (i = 0; i < 1000; i++)
					{
						using (var dr = ctx.GetReader("Select * From Bank..PayDocs Where Handle = @handle",
							"@handle", 3999758))
						{
							dr.Read();
						}
					}
					l.Debug($"SpeedTest 2 Finish {i} count result {i}");
				}
			}
		}
		[Fact]
		public void GetReaderTest()
		{
			using (FileLog l = new FileLog("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				MdbContextOptions.GetOptions().CommandTimeout = 120;
				DateTime date = DateTime.Parse("2014-11-01");
				using (var ctx = new MdbContext(_cn))
				{
					l.Debug("Test 1 Start ");
					int i = 0;
					using (var dr = ctx.GetReader("Select * From Bank..PayDocs Where Handle In @handle",
						"@handle", Enumerable.Range(3999750, 100)))
					{
						for (; dr.Read(); i++)
						{
							l.Debug(dr.GetRowJSON());
						}
					}
					l.Debug($"Test 1 Finish rows result {i}");
					l.Debug("Test 2 Start ");
					i = 0;
					using (var dr = ctx.GetReader("Select * From Bank..PayDocs Where Handle = @handle",
						"@handle", 3999750))
					{
						for (; dr.Read(); i++)
						{
							l.Debug(dr.GetRowJSON());
						}
					}
					l.Debug($"Test 2 Finish rows result {i}");
					l.Debug("Test 3 Start ");
					i = 0;
					using (var dr = ctx.GetReader("Select * From Bank..PayDocs Where DateOper Between @d1 And @d2",
						"@d1", date.AddDays(-4), "@d2", date))
					{
						for (; dr.Read();)
						{
							i++;
						}
					}
					l.Debug($"Test 3 Finish rows result {i}");
					l.Debug("Test 4 Start ");
					i = 0;
					using (var dr = ctx.GetReader("Select * From Bank..PayDocs Where DateOper In @handle",
						"@handle", Enumerable.Range(1, 5).Select(item => date.AddDays(-5).AddDays(item))))
					{
						for (; dr.Read();)
						{
							i++;
						}
					}
					l.Debug($"Test 4 Finish rows result {i}");
					l.Debug("Test 5 Start ");
					i = 0;
					using (var dr = ctx.GetReader(@"Select * From Bank..PayDocs Inner Join Bank..Memorials On PayDocs.DocId = Memorials.DocId 
					Where DateOper In @handle And Contents Like @RUR",
						"@handle", Enumerable.Range(1, 5).Select(item => date.AddDays(-5).AddDays(item)), "@RUR", "%ндс%"))
					{
						for (; dr.Read(); i++)
						{
							l.Debug(dr.GetRowJSON());
						}
					}
					l.Debug($"Test 5 Finish rows result {i}");
					l.Debug("Test 6 Start ");
					i = 0;
					using (var dr = ctx.GetReader("Select * From Bank..PayDocs Where DateOper Between @d1 And @d2",
						new MdbParameter("@d1", date.AddDays(-4)),
						new MdbParameter("@d2", date)))
					{
						for (; dr.Read();)
						{
							i++;
						}
					}
					l.Debug($"Test 6 Finish rows result {i}");
					l.Debug("Test 7 Start ");
					i = 0;
					using (var dr = ctx.GetReader("select * from Bank..VClients where clnum between 37361-100 and 37361"))
					{
						DisplayData(dr.ToDataTable(), l);
					}
					l.Debug($"Test 7 Finish rows result {i}");
				}
			}
		}
		[Fact]
		public void GetReadersTest()
		{
			using (FileLog l = new FileLog("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				MdbContextOptions.GetOptions().CommandTimeout = 120;
				using (var ctx = new MdbContext(_cn))
				{
					l.Debug("Test 1 Start ");
					int i = 0;
					var drs = ctx.GetReaders(@"
						Select * From Bank..PayDocs Where Handle = @handle;
						Select * From Bank..PayDocs Where Handle = @handle1
						Select * From Bank..PayDocs Where Handle = @handle1+1",
						new MdbParameter("@handle", 3999750),
						new MdbParameter("@handle1", 3999751)
						);
					foreach (var dr in drs)
					{
						using (dr)
						{
							for (; dr.Read(); i++)
							{
								l.Debug(dr.GetRowJSON());
							}
						}
					}
					l.Debug($"Test 1 Finish rows result {i}");
				}
			}
		}
		[Fact]
		public void GetSysCatReadersTest()
		{
			using (FileLog l = new FileLog("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				MdbContextOptions.GetOptions().CommandTimeout = 120;
				using (var ctx = new MdbContext(_syscat))
				{
					l.Debug("Test 1 Start ");
					int i = 0;
					var drs = ctx.GetReaders(@"
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
						where u.UserName LIKE '{0}'".ToFormat(@"DESKTOP-14MM2MF\sergey")
						);
					foreach (var dr in drs)
					{
						using (dr)
						{
							for (; dr.Read(); i++)
							{
								l.Debug(dr.GetRowJSON());
							}
						}
					}
					l.Debug($"Test 1 Finish rows result {i}");
				}
			}
		}
		[Fact]
		public void ExecuteTest()
		{
			using (FileLog l = new FileLog("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				MdbContextOptions.GetOptions().CommandTimeout = 120;
				using (var ctx = new MdbContext(_cn))
				{
					l.Debug("Test 1 Start ");
					int i = 0;
					string sql = @"Create Table TestTable(
					ID		uniqueidentifier	not null primary key,
					Name	varchar(128)	not null,
					DateOper	datetime	not null,
					Notes	varchar(256)	null,
					Handle	int				not null)";
					i = ctx.Execute(sql);
					l.Debug($"Test 1 Finish rows result {i}");

					l.Debug("Test 2 Start ");
					sql = @"Insert Into TestTable (ID, Name, DateOper, Handle)
					Values(@ID, @Name, @DateOper, @Handle)";

					i = 0;
					for (; i < 1000; i++)
					{
						ctx.Execute(sql,
							new MdbParameter("@ID", Guid.NewGuid()),
							new MdbParameter("@Name", "Тестовая строка № " + i.ToString()),
							new MdbParameter("@DateOper", vbo.Date().AddDays(i - 1000)),
							new MdbParameter("@Handle", i) { NullIfEmpty = false }); ;
					}
					l.Debug($"Test 2 Finish rows result {i}");
					l.Debug("Test 3 Start ");
					i = 0;
					using (var dr = ctx.GetReader("Select * From TestTable Order By Handle"))
					{
						for (; dr.Read(); i++)
						{
							l.Debug(dr.GetRowJSON());
						}
					}
					l.Debug($"Test 3 Finish rows result {i}");
					l.Debug("Test 4 Start ");
					sql = "Drop Table TestTable";
					i = ctx.Execute(sql);
					l.Debug($"Test 4 Finish rows result {i}");
				}
			}
		}
		[Fact]
		public void TransactionCommitTest()
		{
			TransactionTest(true);
		}
		[Fact]
		public void TransactionRollBackTest()
		{
			TransactionTest(false);
		}

		private void TransactionTest(bool withCommit)
		{
			using (FileLog l = new FileLog("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				using (var ctx = new MdbContext(_cn))
				{
					ctx.BeginTransaction();
					CreateTestTable(ctx, l);
					InsertTestTable(ctx, l);
					SelectTestTable(ctx, l);
					if (withCommit)
					{
						ctx.Commit();
						DropTestTable(ctx, l);
					}
					else
					{
						//Отменяет все коммиты
						ctx.RollBack();
					}
				}
			}
		}

		private static void CreateTestTable(MdbContext ctx, FileLog l)
		{
			l.Debug("Create Test Table Start");
			int i = 0;
			string sql = @"Create Table TestTable(
					ID		uniqueidentifier	not null primary key,
					Name	varchar(128)	not null,
					DateOper	datetime	not null,
					Notes	varchar(256)	null,
					Handle	int				not null)";
			ctx.BeginTransaction();
			i = ctx.Execute(sql);
			ctx.Commit();
		}

		private static void InsertTestTable(MdbContext ctx, FileLog l)
		{
			l.Debug("Insert Test Table Start");
			string sql = @"Insert Into TestTable (ID, Name, DateOper, Handle)
					Values(@ID, @Name, @DateOper, @Handle)";
			ctx.BeginTransaction();
			int i = 0;
			for (; i < 100; i++)
			{
				ctx.Execute(sql,
					new MdbParameter("@ID", Guid.NewGuid()),
					new MdbParameter("@Name", "Тестовая строка № " + i.ToString()),
					new MdbParameter("@DateOper", vbo.Date().AddDays(i - 1000)),
					new MdbParameter("@Handle", i) { NullIfEmpty = false }); ;
			}
			ctx.Commit();
			l.Debug($"Insert Test Table Finish rows result {i}");
		}

		private static void SelectTestTable(MdbContext ctx, FileLog l)
		{
			l.Debug("Select Test Table Start");
			int i = 0;
			using (var dr = ctx.GetReader("Select * From TestTable Order By Handle"))
			{
				for (; dr.Read(); i++)
				{
					l.Debug(dr.GetRowJSON());
				}
			}
			l.Debug($"Select Test Table Finish rows result {i}");
		}

		private static void DropTestTable(MdbContext ctx, FileLog l)
		{
			l.Debug("Drop Test Table Start");
			int i = 0;
			string sql = @"Drop Table TestTable";
			i = ctx.Execute(sql);
			l.Debug($"Drop Test Table Finish rows result {i}");
		}
		//[Fact]
		//public async Task  CreateContextTestAsync()
		//{
		//	await _createContextTestAsync();
		//}
		//async Task _createContextTestAsync()
		//{
		//	using (FileLog l = new FileLog("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
		//	{
		//		Type t = null;
		//		l.Debug("SpeedTest 1 Start ");
		//		int i = 0;
		//		for (i = 0; i < 1000000; i++)
		//		{
		//			using (var ctx = await MdbContext.CreateMdbContextAsync(_cn))
		//			{
		//			}
		//		}
		//		l.Debug($"SpeedTest 1 Finish {i} count result {t}");
		//	}
		//}

		[Fact]
		public void GetReaderSpeedTestAsync()
		{
			_getReaderSpeedTestAsync().GetAwaiter().GetResult();
		}

		private async Task _getReaderSpeedTestAsync()
		{
			using (FileLog l = new FileLog("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				DateTime t = DateTime.MinValue;
				//Caching pool
				int i = 0;
				for (i = 0; i < 100; i++)
				{
					using (var ctx = new MdbContext(_cn))
					{
					}
				}
				using (var ctx = new MdbContext(_cn))
				{
					l.Debug("SpeedTestAsync 1 Start ");
					MdbContextOptions.GetOptions().CommandTimeout = 120;
					for (i = 0; i < 1000; i++)
					{
						using (var dr = await ctx.GetReaderAsync("Select * From Bank..PayDocs Where Handle = @handle",
							new MdbParameter("@handle", 3999758)))
						{
							dr.Read();
						}
					}
					l.Debug($"SpeedTestAsync 1 Finish {i} count result {i}");
					l.Debug("SpeedTestAsync 2 Start ");
					for (i = 0; i < 1000; i++)
					{
						using (var dr = await ctx.GetReaderAsync("Select * From Bank..PayDocs Where Handle = @handle",
							"@handle", 3999758))
						{
							dr.Read();
						}
					}
					l.Debug($"SpeedTestAsync 2 Finish {i} count result {i}");
				}
			}
		}
		[Fact]
		public void GetReadersTestAsync()
		{
			_getReadersTestAsync().GetAwaiter().GetResult();
		}

		private async Task _getReadersTestAsync()
		{
			using (FileLog l = new FileLog("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				MdbContextOptions.GetOptions().CommandTimeout = 120;
				using (var ctx = new MdbContext(_cn))
				{
					l.Debug("Test 1 Start ");
					int i = 0;
					var drs = await ctx.GetReadersAsync(@"
						Select * From Bank..PayDocs Where Handle = @handle;
						Select * From Bank..PayDocs Where Handle = @handle1
						Select * From Bank..PayDocs Where Handle = @handle1+1",
						new MdbParameter("@handle", 3999750),
						new MdbParameter("@handle1", 3999751)
						);
					foreach (var dr in drs)
					{
						using (dr)
						{
							for (; dr.Read(); i++)
							{
								l.Debug(dr.GetRowJSON());
							}
						}
					}
					l.Debug($"Test 1 Finish rows result {i}");
				}
			}
		}
		[Fact]
		public async Task ExecuteTestAsinc()
		{
			await executeTestAsync();
		}

		private async Task executeTestAsync()
		{
			using (FileLog l = new FileLog("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				MdbContextOptions.GetOptions().CommandTimeout = 120;
				using (var ctx = new MdbContext(_cn))
				{
					l.Debug("Test 1 Start ");
					int i = 0;
					string sql = @"Create Table TestTable(
					ID		uniqueidentifier	not null primary key,
					Name	varchar(128)	not null,
					DateOper	datetime	not null,
					Notes	varchar(256)	null,
					Handle	int				not null)";
					i = await ctx.ExecuteAsync(sql);
					l.Debug($"Test 1 Finish rows result {i}");

					l.Debug("Test 2 Start ");
					sql = @"Insert Into TestTable (ID, Name, DateOper, Handle)
					Values(@ID, @Name, @DateOper, @Handle)";

					i = 0;
					for (; i < 1000; i++)
					{
						await ctx.ExecuteAsync(sql,
							new MdbParameter("@ID", Guid.NewGuid()),
							new MdbParameter("@Name", "Тестовая строка № " + i.ToString()),
							new MdbParameter("@DateOper", vbo.Date().AddDays(i - 1000)),
							new MdbParameter("@Handle", i) { NullIfEmpty = false }); ;
					}
					l.Debug($"Test 2 Finish rows result {i}");
					l.Debug("Test 3 Start ");
					i = 0;
					using (var dr = await ctx.GetReaderAsync("Select * From TestTable Order By Handle"))
					{
						for (; dr.Read(); i++)
						{
							l.Debug(dr.GetRowJSON());
						}
					}
					l.Debug($"Test 3 Finish rows result {i}");
					l.Debug("Test 4 Start ");
					sql = "Drop Table TestTable";
					i = await ctx.ExecuteAsync(sql);
					l.Debug($"Test 4 Finish rows result {i}");
				}
			}
		}
		[Fact]
		public async Task TransactionCommitTestAsync()
		{
			await _TransactionTestAsync(true);
		}
		[Fact]
		public async Task TransactionRollbackTestAsync()
		{
			await _TransactionTestAsync(false);
		}

		private async Task _TransactionTestAsync(bool withCommit)
		{
			using (FileLog l = new FileLog("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				using (var ctx = new MdbContext(_cn))
				{
					await ctx.BeginTransactionAsync();
					await CreateTestTableAsync(ctx, l);
					await InsertTestTableAsync(ctx, l);
					await SelectTestTableAsync(ctx, l);
					if (withCommit)
					{
						await ctx.CommitAsync();
						await DropTestTableAsync(ctx, l);
					}
					else
					{
						//Отменяет все коммиты
						ctx.RollBack();
					}
				}
			}
		}

		private static async Task CreateTestTableAsync(MdbContext ctx, FileLog l)
		{
			l.Debug("Create Test Table Start");
			int i = 0;
			string sql = @"Create Table TestTable(
					ID		uniqueidentifier	not null primary key,
					Name	varchar(128)	not null,
					DateOper	datetime	not null,
					Notes	varchar(256)	null,
					Handle	int				not null)";
			await ctx.BeginTransactionAsync();
			i = await ctx.ExecuteAsync(sql);
			await ctx.CommitAsync();
		}

		private static async Task InsertTestTableAsync(MdbContext ctx, FileLog l)
		{
			l.Debug("Insert Test Table Start");
			string sql = @"Insert Into TestTable (ID, Name, DateOper, Handle)
					Values(@ID, @Name, @DateOper, @Handle)";
			await ctx.BeginTransactionAsync();
			int i = 0;
			for (; i < 100; i++)
			{
				await ctx.ExecuteAsync(sql,
					new MdbParameter("@ID", Guid.NewGuid()),
					new MdbParameter("@Name", "Тестовая строка № " + i.ToString()),
					new MdbParameter("@DateOper", vbo.Date().AddDays(i - 1000)),
					new MdbParameter("@Handle", i) { NullIfEmpty = false }); ;
			}
			await ctx.CommitAsync();
			l.Debug($"Insert Test Table Finish rows result {i}");
		}

		private static async Task SelectTestTableAsync(MdbContext ctx, FileLog l)
		{
			l.Debug("Select Test Table Start");
			int i = 0;
			using (var dr = await ctx.GetReaderAsync("Select * From TestTable Order By Handle"))
			{
				for (; dr.Read(); i++)
				{
					l.Debug(dr.GetRowJSON());
				}
			}
			l.Debug($"Select Test Table Finish rows result {i}");
		}

		private static async Task DropTestTableAsync(MdbContext ctx, FileLog l)
		{
			l.Debug("Drop Test Table Start");
			int i = 0;
			string sql = @"Drop Table TestTable";
			i = await ctx.ExecuteAsync(sql);
			l.Debug($"Drop Test Table Finish rows result {i}");
		}
		private static void DisplayData(System.Data.DataTable table, FileLog l)
		{
			StringBuilder sb = new StringBuilder();
			foreach (System.Data.DataRow row in table.Rows)
			{
				foreach (System.Data.DataColumn col in table.Columns)
				{
					sb.AppendFormat("{0} = {1}\t", col.ColumnName, row[col]);
				}
				sb.AppendLine();
			}
			l.Debug(sb.ToString());
		}
	}
}
