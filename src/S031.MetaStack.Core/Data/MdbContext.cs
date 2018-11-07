using System;
using System.Collections.Concurrent;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using S031.MetaStack.Common;
using System.Data;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace S031.MetaStack.Core.Data
{
	public enum MdbAdapters
	{
		SqlServer,
		PostgreSql
	}


	public class MdbContext : IDisposable
	{
		private DbTransaction _transaction;
		private int _transactionLevel = 0;
		private ConnectInfo _connectInfo;

		private MdbContext() { }
		/// <summary>
		/// Create new <see cref="S031.MetaStack.Core.Data.MdbContext"/> with specified connection string and logger
		/// </summary>
		/// <param name="connectionString">connection string with Provider Name item if you don't use default</param>
		public MdbContext(string connectionString)
		{
			connectionString.NullTest(nameof(connectionString));
			_connectInfo = new ConnectInfo(connectionString);
			Factory = ObjectFactories.GetFactory<DbProviderFactory>(_connectInfo.ProviderName);
			Connection = Factory.CreateConnection(_connectInfo.ConnectionString);
		}
		public string ProviderName => _connectInfo.ProviderName;
		public string DbName => _connectInfo.DbName;
		public DbProviderFactory Factory { get; private set; }
		public DbConnection Connection { get; private set; }
		/// <summary>
		/// Run <see cref="DbCommand.ExecuteReader()"/> and returns the data in the format <see cref="DataPackage"/>
		/// </summary>
		/// <param name="sql">SQL string for command</param>
		/// <returns></returns>
		public DataPackage GetReader(string sql) => GetReader(sql, new MdbParameter[] { });
		/// <summary>
		/// Run <see cref="DbCommand.ExecuteReader()"/> and returns the data in the format <see cref="DataPackage"/>
		/// </summary>
		/// <param name="sql">SQL string for command</param>
		/// <param name="parameters">Array of <see cref="MdbAdapters"/></param>
		/// <returns></returns>
		public DataPackage GetReader(string sql, params MdbParameter[] parameters) => GetReaders(sql, parameters)[0];
		/// <summary>
		/// Run <see cref="DbCommand.ExecuteReader()"/> for multiple statements and returns array of <see cref="DataPackage"/>
		/// </summary>
		/// <param name="sql">SQL string for command with multiple statements</param>
		/// <param name="parameters">Array of <see cref="MdbAdapters"/></param>
		/// <returns></returns>
		public DataPackage[] GetReaders(string sql, params MdbParameter[] parameters)
		{
			List<DataPackage> rs = new List<Data.DataPackage>();
			using (DbCommand command = getCommandInternal(sql, parameters))
			{
				using (DbDataReader dr = command.ExecuteReader())
				{
					rs.Add(new DataPackage(DataPackage.WriteData(dr)));
					for (; dr.NextResult();)
						rs.Add(new DataPackage(DataPackage.WriteData(dr)));
				}
			}
			return rs.ToArray();
		}
		/// <summary>
		/// Run <see cref="DbCommand.ExecuteReader()"/> and returns the data in the format <see cref="DataPackage"/>
		/// </summary>
		/// <param name="sql">SQL string for command</param>
		/// <param name="ps">param array in sequence as key1, value1, key2, value2 ... 
		/// where key - parameter name, value - parameter value</param>
		/// <returns></returns>
		public DataPackage GetReader(string sql, params object[] ps) =>
			GetReader(sql, ParamArray2MdbParamArray(ps));

		static MdbParameter[] ParamArray2MdbParamArray(params object[] ps)
		{
			int i = 0;
			string pName = string.Empty;
			string pValue = string.Empty;
			List<MdbParameter> pList = new List<MdbParameter>();
			foreach (var p in ps)
			{
				if (i % 2 == 0)
					pName = (string)p;
				else
					pList.Add(new MdbParameter(pName, p));
				i++;
			}
			return pList.ToArray();
		}
		static string array2List(object a)
		{
			Type t = a.GetType().GetInterfaces().FirstOrDefault(type => type.Name == "IEnumerable`1");

			if (t == null)
				// Parameter a nust implements IEnumerable<> interface
				throw new ArgumentException("S031.MetaStack.Core.Data.MdbContext.array2List.1");

			t = t.GetGenericArguments()[0];
			if (t == typeof(string))
			{
				IEnumerable<string> aStr = (IEnumerable<string>)a;
				return "('" + string.Join("','", aStr) + "')";
			}
			else if (t == typeof(int))
			{
				IEnumerable<int> aInt = (IEnumerable<int>)a;
				return "(" + string.Join(",", aInt.Select(i => i.ToString())) + ")";
			}
			else if (t == typeof(byte))
			{
				IEnumerable<byte> aByte = (IEnumerable<byte>)a;
				return "(" + string.Join(",", aByte.Select(i => i.ToString())) + ")";
			}
			else if (t == typeof(decimal))
			{
				IEnumerable<decimal> aDecimal = (IEnumerable<decimal>)a;
				return "(" + string.Join(",", aDecimal.Select(i => i.ToString())) + ")";
			}
			else if (t == typeof(double))
			{
				IEnumerable<double> aDouble = (IEnumerable<double>)a;
				return "(" + string.Join(",", aDouble.Select(i => i.ToString())) + ")";
			}
			else if (t == typeof(DateTime))
			{
				IEnumerable<DateTime> aDate = (IEnumerable<DateTime>)a;
				return "('" + string.Join("','", aDate.Select(i => i.ToString(vbo.DBDateFormat))) + "')";
			}
			else if (t == typeof(object))
			{
				IEnumerable<object> aDate = (IEnumerable<object>)a;
				return "('" + string.Join("','", aDate.Select(i => i.ToString())) + "')";
			}
			// IEnumerable <{0}> argument type not supported
			throw new ArgumentException("S031.MetaStack.Core.Data.MdbContext.array2List.2".ToFormat(t.Name));
		}
		/// <summary>
		/// Run <see cref="DbCommand.ExecuteNonQuery"/>
		/// </summary>
		/// <param name="sql">SQL command</param>
		/// <param name="parameters">Array of <see cref="MdbAdapters"</param>
		/// <returns><see cref="int"/></returns>
		public int Execute(string sql, params MdbParameter[] parameters)
		{
			using (DbCommand command = getCommandInternal(sql, parameters))
			{
				return command.ExecuteNonQuery();
			}
		}
		/// <summary>
		/// Run (T)<see cref="DbCommand.ExecuteScalar"/>
		/// Returns max lenght 2033
		/// </summary>
		/// <param name="sql">SQL command</param>
		/// <param name="parameters">Array of <see cref="MdbAdapters"</param>
		/// <returns><see cref="int"/></returns>
		public T Execute<T>(string sql, params MdbParameter[] parameters)
		{
			using (DbCommand command = getCommandInternal(sql, parameters))
			{
				return (T)command.ExecuteScalar();
			}
		}
		DbCommand getCommandInternal(string sql, params MdbParameter[] parameters)
		{
			if (Connection.State != ConnectionState.Open)
				Connection.Open();
			CommandType ct = sql.IndexOf(' ') == -1 ? CommandType.StoredProcedure : CommandType.Text;

			List<MdbParameter> pFact = new List<MdbParameter>();
			foreach (var param in parameters)
			{
				bool replace = sql.IndexOf(param.Name) > -1;
				if (replace)
				{
					object p = param.Value;
					string pValue = string.Empty;
					Type t = p.GetType();
					if (t.IsNumeric())
						pValue = p.ToString();
					else if (t == typeof(DateTime))
						//!!! full date format
						pValue = "'" + string.Format("{0:yyyyMMdd}", p) + "'";
					else if (t == typeof(string))
						pValue = "'" + p.ToString().Replace("'", "''") + "'";
					else if (t != typeof(byte[]) && t.GetInterfaces().FirstOrDefault(type => type.Name == "IEnumerable`1") != null)
						pValue = array2List(p);
					else
						replace = false;

					if (replace)
						sql = sql.Replace(param.Name, pValue);
					else
						pFact.Add(param);
				}
				else
					pFact.Add(param);
			}

			DbCommand command = Factory.CreateCommand(Connection, sql);
			command.CommandType = ct;
			command.CommandTimeout = MdbContextOptions.GetOptions().CommandTimeout;
			if (_transaction != null)
				command.Transaction = _transaction;

			foreach (var param in pFact)
				command.AddParameter(param.Name, param.Value);
			return command;
		}
		/// <summary>
		/// Start a database transaction with isolation level specified in <see cref="MdbContextOptions.TransactionIsolationLevel"/>
		/// </summary>
		public void BeginTransaction()
		{
			if (_transaction == null)
			{
				if (Connection.State != ConnectionState.Open)
					Connection.Open();
				_transaction = Connection.BeginTransaction(MdbContextOptions.GetOptions().TransactionIsolationLevel);
				_transactionLevel++;
			}
			else
				_transactionLevel++;
		}
		/// <summary>
		/// Commit the database transaction
		/// </summary>
		public void Commit()
		{
			if (_transactionLevel > 1)
				_transactionLevel--;
			else if (_transaction != null)
			{
				_transaction.Commit();
				_transaction = null;
				_transactionLevel = 0;
			}
			else
				// Can't call Commit method without transaction.
				throw new InvalidOperationException("S031.MetaStack.Core.Data.MdbContext.Commit.1");
		}
		/// <summary>
		/// Rolls back a transaction and pending state
		/// </summary>
		public void RollBack()
		{
			_transaction.Rollback();
			_transactionLevel = 0;
			_transaction = null;
		}
		/// <summary>
		/// <see cref="IDisposable.Dispose()"/>
		/// </summary>
		public void Dispose()
		{
			_transaction?.Dispose();
			if (Connection?.State == ConnectionState.Open)
				Connection.Close();
			Connection?.Dispose();
		}
		#region async_methods
		/// <summary>
		/// Async constructor for <see cref="MdbContext" object/>
		/// </summary>
		/// <param name="connectionString">connection string with Provider Name item if you don't use default </param>
		/// <param name="logger"><see cref="ILogger"/></param>
		/// <returns></returns>
		//public static async Task<MdbContext> CreateMdbContextAsync(string connectionString)
		//{
		//	//Не рекомендуется в async методе выбрасывать исключения при проверке аргументов.
		//	connectionString.NullTest(nameof(connectionString));
		//	MdbContext mctx = new MdbContext
		//	{
		//		_connectInfo = new ConnectInfo(connectionString)
		//	};
		//	mctx.Factory = ObjectFactories.GetFactory<DbProviderFactory>(mctx._connectInfo.ProviderName);
		//	mctx.Connection = await mctx.Factory.CreateConnectionAsync(mctx._connectInfo.ConnectionString);
		//	return mctx;
		//}
		/// <summary>
		/// Async version of <see cref="MdbContext.Execute(string, MdbParameter[])"/>
		/// </summary>
		public async Task<int> ExecuteAsync(string sql, params MdbParameter[] parameters)
		{
			using (DbCommand command = getCommandInternal(sql, parameters))
			{
				return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
			}
		}
		public async Task<T> ExecuteAsync<T>(string sql, params MdbParameter[] parameters)
		{
			using (DbCommand command = getCommandInternal(sql, parameters))
			{
				var result = await command.ExecuteScalarAsync().ConfigureAwait(false);
				return (T)result;
			}
		}
		/// <summary>
		/// Async version of <see cref="MdbContext.GetReaders(string, MdbParameter[])"/>
		/// </summary>
		public async Task<DataPackage[]> GetReadersAsync(string sql, params MdbParameter[] parameters)
		{
			List<DataPackage> rs = new List<Data.DataPackage>();
			using (DbCommand command = getCommandInternal(sql, parameters))
			{
				using (DbDataReader dr = await command.ExecuteReaderAsync().ConfigureAwait(false))
				{
					rs.Add(new DataPackage(DataPackage.WriteData(dr)));
					for (; await dr.NextResultAsync().ConfigureAwait(false);)
						rs.Add(new DataPackage(DataPackage.WriteData(dr)));
				}
			}
			return rs.ToArray();
		}
		/// <summary>
		/// Async version of <see cref="MdbContext.GetReader(string)"/>
		/// </summary>
		public async Task<DataPackage> GetReaderAsync(string sql) => await GetReaderAsync(sql, new MdbParameter[] { }).ConfigureAwait(false);
		/// <summary>
		/// Async version of <see cref="MdbContext.GetReader(string, MdbParameter[])"/>
		/// </summary>
		public async Task<DataPackage> GetReaderAsync(string sql, params MdbParameter[] parameters) =>
			(await GetReadersAsync(sql, parameters).ConfigureAwait(false))[0];
		/// <summary>
		/// Async version of <see cref="MdbContext.GetReader(string, object[])"/>
		/// </summary>
		public async Task<DataPackage> GetReaderAsync(string sql, params object[] ps) =>
			await GetReaderAsync(sql, ParamArray2MdbParamArray(sql, ps)).ConfigureAwait(false);
		/// <summary>
		/// Crutch
		/// https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/asynchronous-programming
		/// </summary>
		/// <returns></returns>
		public async Task BeginTransactionAsync()
		{
			if (Connection.State != ConnectionState.Open)
				await Connection.OpenAsync();
			BeginTransaction();
		}
		/// <summary>
		/// Crutch To
		/// https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/asynchronous-programming
		/// </summary>
		/// <returns></returns>
		public async Task CommitAsync()
		{
			await Task.Run(() => Commit()).ConfigureAwait(false);
		}
		#endregion async_methods
	}

	public class MdbContextOptions
	{
		static readonly MdbContextOptions _options = new MdbContextOptions();
		/// <summary>
		/// ctor with defaults
		/// </summary>
		private MdbContextOptions()
		{
			this.CommandTimeout = 60;
			this.TransactionIsolationLevel = IsolationLevel.ReadCommitted;
		}
		/// <summary>
		/// Get <see cref="MdbContextOptions" /> singleton
		/// </summary>
		/// <returns><see cref="MdbContextOptions" /></returns>
		public static MdbContextOptions GetOptions() => _options;
		/// <summary>
		/// Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
		/// <see cref="DbCommand.CommandTimeout"/>
		/// </summary>
		public int CommandTimeout { get; set; }
		/// <summary>
		/// Get, Set <see cref="IsolationLevel"/>  for database transactions 
		/// </summary>
		public IsolationLevel TransactionIsolationLevel{ get; set; }

	}
}

