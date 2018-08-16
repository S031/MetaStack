using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using S031.MetaStack.Common;
using S031.MetaStack.Common.Logging;
using S031.MetaStack.Core.Logging;
using S031.MetaStack.Core.App;
using System.Linq;
using S031.MetaStack.Core.Data;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace MetaStack.UnitTest.Data
{
	[TestClass]
	public class MdbContextTest
	{
		const string connection_name = "bankLocal";
		readonly string _cn;
		readonly string _providerName;
		readonly IConfiguration _configuration;

		internal string providerName => _providerName;
		internal string connectionString => _cn;
		public MdbContextTest()
		{
			_configuration = new ConfigurationBuilder()
				.AddJsonFile("config.json", optional: false, reloadOnChange: true)
				.Build();

			FileLogSettings.Default.Filter = (s, i) => i >= LogLevels.Debug;
			var cs = _configuration.GetSection($"connectionStrings:{connection_name}");
			_providerName = (string)cs.GetValue<string>("providerName");
			_cn = MdbContext.CreateConnectionString(_providerName,
				(string)cs.GetValue<string>("connectionString"));
		}
		[TestMethod]
		public void createContextTest()
		{
			using (FileLogger l = new FileLogger("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				Type t = null;
				l.Debug("SpeedTest 1 Start ");
				int i = 0;
				for (i = 0; i < 1000000; i++)
				{
					using (var ctx = new MdbContext(_cn, l))
					{
					}
				}
				l.Debug($"SpeedTest 1 Finish {i} count result {t}");
			}
		}
		[TestMethod]
		public void GetReaderSpeedTest()
		{
			using (FileLogger l = new FileLogger("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				DateTime t = DateTime.MinValue;
				//Caching pool
				int i = 0;
				for (i = 0; i < 100; i++)
				{
					using (var ctx = new MdbContext(_cn, l))
					{
					}
				}
				using (var ctx = new MdbContext(_cn, l))
				{
					l.Debug("SpeedTest 1 Start ");
					MdbContextOptions.GetOptions().CommandTimeout = 120;
					for (i = 0; i < 1000; i++)
					{
						using (var dr = ctx.GetReader("Select * From PayDocs Where Handle = @handle",
							new MdbParameter("@handle", 3999758)))
						{
							dr.Read();
						}
					}
					l.Debug($"SpeedTest 1 Finish {i} count result {i}");
					l.Debug("SpeedTest 2 Start ");
					for (i = 0; i < 1000; i++)
					{
						using (var dr = ctx.GetReader("Select * From PayDocs Where Handle = @handle",
							"@handle", 3999758))
						{
							dr.Read();
						}
					}
					l.Debug($"SpeedTest 2 Finish {i} count result {i}");
				}
			}
		}
		[TestMethod]
		public void GetReaderTest()
		{
			using (FileLogger l = new FileLogger("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				MdbContextOptions.GetOptions().CommandTimeout = 120;
				using (var ctx = new MdbContext(_cn, l))
				{
					l.Debug("Test 1 Start ");
					int i = 0;
					using (var dr = ctx.GetReader("Select * From PayDocs Where Handle In @handle",
						"@handle", Enumerable.Range(3999750, 100)))
					{
						for (; dr.Read(); i++)
							l.Debug(dr.GetRowJSON());
					}
					l.Debug($"Test 1 Finish rows result {i}");
					l.Debug("Test 2 Start ");
					i = 0;
					using (var dr = ctx.GetReader("Select * From PayDocs Where Handle = @handle",
						"@handle", 3999750))
					{
						for (; dr.Read(); i++)
							l.Debug(dr.GetRowJSON());
					}
					l.Debug($"Test 2 Finish rows result {i}");
					l.Debug("Test 3 Start ");
					i = 0;
					using (var dr = ctx.GetReader("Select * From PayDocs Where DateOper Between @d1 And @d2",
						"@d1", vbo.Date().AddDays(-4), "@d2", vbo.Date()))
					{
						for (; dr.Read();)
							i++;
					}
					l.Debug($"Test 3 Finish rows result {i}");
					l.Debug("Test 4 Start ");
					i = 0;
					using (var dr = ctx.GetReader("Select * From PayDocs Where DateOper In @handle",
						"@handle", Enumerable.Range(1, 5).Select(item => vbo.Date().AddDays(-5).AddDays(item))))
					{
						for (; dr.Read();)
							i++;
					}
					l.Debug($"Test 4 Finish rows result {i}");
					l.Debug("Test 5 Start ");
					i = 0;
					using (var dr = ctx.GetReader(@"Select * From PayDocs Inner Join Memorials On PayDocs.DocId = Memorials.DocId 
					Where DateOper In @handle And Contents Like @RUR",
						"@handle", Enumerable.Range(1, 5).Select(item => vbo.Date().AddDays(-5).AddDays(item)), "@RUR", "%'RUB'%"))
					{
						for (; dr.Read(); i++)
							l.Debug(dr.GetRowJSON());
					}
					l.Debug($"Test 5 Finish rows result {i}");
					l.Debug("Test 6 Start ");
					i = 0;
					using (var dr = ctx.GetReader("Select * From PayDocs Where DateOper Between @d1 And @d2",
						new MdbParameter("@d1", vbo.Date().AddDays(-4)),
						new MdbParameter("@d2", vbo.Date())))
					{
						for (; dr.Read();)
							i++;
					}
					l.Debug($"Test 6 Finish rows result {i}");
					l.Debug("Test 7 Start ");
					i = 0;
					using (var dr = ctx.GetReader("select * from VClients where clnum between 37361-100 and 37361"))
					{
						DisplayData(dr.ToDataTable(), l);
					}
					l.Debug($"Test 7 Finish rows result {i}");
				}
			}
		}
		[TestMethod]
		public void GetReadersTest()
		{
			using (FileLogger l = new FileLogger("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				MdbContextOptions.GetOptions().CommandTimeout = 120;
				using (var ctx = new MdbContext(_cn, l))
				{
					l.Debug("Test 1 Start ");
					int i = 0;
					var drs = ctx.GetReaders(@"
						Select * From PayDocs Where Handle = @handle;
						Select * From PayDocs Where Handle = @handle1
						Select * From PayDocs Where Handle = @handle1+1",
						new MdbParameter("@handle", 3999750),
						new MdbParameter("@handle1", 3999751)
						);
					foreach (var dr in drs)
					{
						using (dr)
						{
							for (; dr.Read(); i++)
								l.Debug(dr.GetRowJSON());
						}
					}
					l.Debug($"Test 1 Finish rows result {i}");
				}
			}
		}
		[TestMethod]
		public void ExecuteTest()
		{
			using (FileLogger l = new FileLogger("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				MdbContextOptions.GetOptions().CommandTimeout = 120;
				using (var ctx = new MdbContext(_cn, l))
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
							l.Debug(dr.GetRowJSON());
					}
					l.Debug($"Test 3 Finish rows result {i}");
					l.Debug("Test 4 Start ");
					sql = "Drop Table TestTable";
					i = ctx.Execute(sql);
					l.Debug($"Test 4 Finish rows result {i}");
				}
			}
		}
		[TestMethod]
		public void TransactionCommitTest()
		{
			TransactionTest(true);
		}
		[TestMethod]
		public void TransactionRollBackTest()
		{
			TransactionTest(false);
		}
		void TransactionTest(bool withCommit)
		{
			using (FileLogger l = new FileLogger("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				using (var ctx = new MdbContext(_cn, l))
				{
					ctx.BeginTransaction();
					createTestTable(ctx);
					insertTestTable(ctx);
					selectTestTable(ctx);
					if (withCommit)
					{
						ctx.Commit();
						dropTestTable(ctx);
					}
					else
						//Отменяет все коммиты
						ctx.RollBack();
				}
			}
		}
		static void createTestTable(MdbContext ctx)
		{
			FileLogger l = (ctx.Logger as FileLogger);
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
		static void insertTestTable(MdbContext ctx)
		{
			FileLogger l = (ctx.Logger as FileLogger);
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
		static void selectTestTable(MdbContext ctx)
		{
			FileLogger l = (ctx.Logger as FileLogger);
			l.Debug("Select Test Table Start");
			int i = 0;
			using (var dr = ctx.GetReader("Select * From TestTable Order By Handle"))
			{
				for (; dr.Read(); i++)
					l.Debug(dr.GetRowJSON());
			}
			l.Debug($"Select Test Table Finish rows result {i}");
		}
		static void dropTestTable(MdbContext ctx)
		{
			FileLogger l = (ctx.Logger as FileLogger);
			l.Debug("Drop Test Table Start");
			int i = 0;
			string sql = @"Drop Table TestTable";
			i = ctx.Execute(sql);
			l.Debug($"Drop Test Table Finish rows result {i}");
		}
		[TestMethod]
		public async Task  CreateContextTestAsync()
		{
			await _createContextTestAsync();
		}
		async Task _createContextTestAsync()
		{
			using (FileLogger l = new FileLogger("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				Type t = null;
				l.Debug("SpeedTest 1 Start ");
				int i = 0;
				for (i = 0; i < 1000000; i++)
				{
					using (var ctx = await MdbContext.CreateMdbContextAsync(_cn, l))
					{
					}
				}
				l.Debug($"SpeedTest 1 Finish {i} count result {t}");
			}
		}
		[TestMethod]
		public void GetReaderSpeedTestAsync()
		{
			_getReaderSpeedTestAsync().GetAwaiter().GetResult();
		}
		async Task _getReaderSpeedTestAsync()
		{
			using (FileLogger l = new FileLogger("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				DateTime t = DateTime.MinValue;
				//Caching pool
				int i = 0;
				for (i = 0; i < 100; i++)
				{
					using (var ctx = await MdbContext.CreateMdbContextAsync(_cn, l))
					{
					}
				}
				using (var ctx = await MdbContext.CreateMdbContextAsync(_cn, l))
				{
					l.Debug("SpeedTestAsync 1 Start ");
					MdbContextOptions.GetOptions().CommandTimeout = 120;
					for (i = 0; i < 1000; i++)
					{
						using (var dr = await ctx.GetReaderAsync("Select * From PayDocs Where Handle = @handle",
							new MdbParameter("@handle", 3999758)))
						{
							dr.Read();
						}
					}
					l.Debug($"SpeedTestAsync 1 Finish {i} count result {i}");
					l.Debug("SpeedTestAsync 2 Start ");
					for (i = 0; i < 1000; i++)
					{
						using (var dr = await ctx.GetReaderAsync("Select * From PayDocs Where Handle = @handle",
							"@handle", 3999758))
						{
							dr.Read();
						}
					}
					l.Debug($"SpeedTestAsync 2 Finish {i} count result {i}");
				}
			}
		}
		[TestMethod]
		public void GetReadersTestAsync()
		{
			_getReadersTestAsync().GetAwaiter().GetResult();
		}
		async Task _getReadersTestAsync()
		{
			using (FileLogger l = new FileLogger("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				MdbContextOptions.GetOptions().CommandTimeout = 120;
				using (var ctx = new MdbContext(_cn, l))
				{
					l.Debug("Test 1 Start ");
					int i = 0;
					var drs = await ctx.GetReadersAsync(@"
						Select * From PayDocs Where Handle = @handle;
						Select * From PayDocs Where Handle = @handle1
						Select * From PayDocs Where Handle = @handle1+1",
						new MdbParameter("@handle", 3999750),
						new MdbParameter("@handle1", 3999751)
						);
					foreach (var dr in drs)
					{
						using (dr)
						{
							for (; dr.Read(); i++)
								l.Debug(dr.GetRowJSON());
						}
					}
					l.Debug($"Test 1 Finish rows result {i}");
				}
			}
		}
		[TestMethod]
		public async Task ExecuteTestAsinc()
		{
			await executeTestAsync();
		}
		async Task executeTestAsync()
		{
			using (FileLogger l = new FileLogger("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				MdbContextOptions.GetOptions().CommandTimeout = 120;
				using (var ctx = await MdbContext.CreateMdbContextAsync(_cn, l))
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
							l.Debug(dr.GetRowJSON());
					}
					l.Debug($"Test 3 Finish rows result {i}");
					l.Debug("Test 4 Start ");
					sql = "Drop Table TestTable";
					i = await ctx.ExecuteAsync(sql);
					l.Debug($"Test 4 Finish rows result {i}");
				}
			}
		}
		[TestMethod]
		public async Task TransactionCommitTestAsync()
		{
			await _TransactionTestAsync(true);
		}
		[TestMethod]
		public async Task TransactionRollbackTestAsync()
		{
			await _TransactionTestAsync(false);
		}
		async Task _TransactionTestAsync(bool withCommit)
		{
			using (FileLogger l = new FileLogger("MdbContextTest", new FileLogSettings() { DateFolderMask = "yyyy-MM-dd" }))
			{
				using (var ctx = new MdbContext(_cn, l))
				{
					await ctx.BeginTransactionAsync();
					await createTestTableAsync(ctx);
					await insertTestTableAsync(ctx);
					await selectTestTableAsync(ctx);
					if (withCommit)
					{
						await ctx.CommitAsync();
						await dropTestTableAsync(ctx);
					}
					else
						//Отменяет все коммиты
						ctx.RollBack();
				}
			}
		}
		static async Task createTestTableAsync(MdbContext ctx)
		{
			FileLogger l = (ctx.Logger as FileLogger);
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
		static async Task insertTestTableAsync(MdbContext ctx)
		{
			FileLogger l = (ctx.Logger as FileLogger);
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
		static async Task selectTestTableAsync(MdbContext ctx)
		{
			FileLogger l = (ctx.Logger as FileLogger);
			l.Debug("Select Test Table Start");
			int i = 0;
			using (var dr = await ctx.GetReaderAsync("Select * From TestTable Order By Handle"))
			{
				for (; dr.Read(); i++)
					l.Debug(dr.GetRowJSON());
			}
			l.Debug($"Select Test Table Finish rows result {i}");
		}
		static async Task dropTestTableAsync(MdbContext ctx)
		{
			FileLogger l = (ctx.Logger as FileLogger);
			l.Debug("Drop Test Table Start");
			int i = 0;
			string sql = @"Drop Table TestTable";
			i = await ctx.ExecuteAsync(sql);
			l.Debug($"Drop Test Table Finish rows result {i}");
		}
		private static void DisplayData(System.Data.DataTable table, FileLogger l)
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
