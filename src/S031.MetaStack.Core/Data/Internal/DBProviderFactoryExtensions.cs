using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Common;

namespace S031.MetaStack.Core
{
    internal static class DBProviderFactoryExtensions
    {
		/// <summary>
		/// Create and Open <see cref="DbConnection"/>
		/// </summary>
		/// <param name="factory"><see cref="DbProviderFactory"/></param>
		/// <param name="connectionString"><see cref="DbConnection.ConnectionString"/></param>
		/// <returns></returns>
		public static DbConnection CreateConnection(this DbProviderFactory factory, string connectionString)
		{
			var c = factory.CreateConnection();
			c.ConnectionString = connectionString;
			c.Open();
			return c;
		}
		/// <summary>
		/// Async Create and Open <see cref="DbConnection"/>
		/// </summary>
		/// <param name="factory"><see cref="DbProviderFactory"/></param>
		/// <param name="connectionString"><see cref="DbConnection.ConnectionString"/></param>
		/// <returns></returns>
		public static async Task<DbConnection> CreateConnectionAsync(this DbProviderFactory factory, string connectionString)
		{
			var c = factory.CreateConnection();
			c.ConnectionString = connectionString;
			await c.OpenAsync().ConfigureAwait(false);
			return c;
		}
		/// <summary>
		/// Create <see cref="DbCommand"/>
		/// </summary>
		/// <param name="factory"><see cref="DbProviderFactory"/></param>
		/// <param name="connection"><see cref="DbConnection"/></param>
		/// <param name="sql">SQL command string</param>
		/// <returns></returns>
		public static DbCommand CreateCommand(this DbProviderFactory factory, DbConnection connection, string sql)
		{
			var c = factory.CreateCommand();
			c.Connection = connection;
			c.CommandText = sql;
			return c;
		}
    }
	internal static class DBCommandExtensions
	{
		public static void AddParameter(this DbCommand command, string name, object value)
		{
			var parameter = command.CreateParameter();
			parameter.ParameterName = name;
			parameter.Value = value;
			command.Parameters.Add(parameter);
		}
	}
}
