using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace S031.MetaStack.Data
{
	internal static class DbProviderFactories
	{
		//fast cache for several types
		private static DbProviderFactory _sql = null;
		private static DbProviderFactory _sqlite = null;
		private static DbProviderFactory _sybase = null;
		private static DbProviderFactory _odbc = null;
		//all other
		private static readonly MapTable<string, DbProviderFactory> _factories = new MapTable<string, DbProviderFactory>();
		private static readonly object obj4Lock = new object();
		
		/// <summary>
		/// Return instance of <see cref="System.Data.Common.DbProviderFactory"/> for specified provider name 
		/// </summary>
		/// <param name="providerInvariantName">Provider Name aka <see cref="System.Data.SqlClient"</param>
		/// <returns></returns>
		public static DbProviderFactory GetFactory(string providerInvariantName)
		{

			if (providerInvariantName.Equals("System.Data.SqlClient", StringComparison.OrdinalIgnoreCase))
			{
				if (_sql == null)
				{
					lock (obj4Lock)
						_sql = LoadFromAssembly("System.Data.SqlClient", "System.Data.SqlClient.SqlClientFactory");
				}
				return _sql;
			}
			else if (providerInvariantName.Equals("System.Data.SQLite", StringComparison.OrdinalIgnoreCase))
			{
				if (_sqlite == null)
					lock (obj4Lock)
						_sqlite = LoadFromAssembly("System.Data.SQLite", "System.Data.SQLite.SQLiteFactory");
				return _sqlite;
			}
			else if (providerInvariantName.Equals("Sybase.Data.AseClient", StringComparison.OrdinalIgnoreCase))
			{
				if (_sybase == null)
					lock (obj4Lock)
						_sybase = LoadFromAssembly("Sybase.AdoNet4.AseClient", "Sybase.Data.AseClient.AseClientFactory");
				return _sybase;
			}
			else if (providerInvariantName.Equals("System.Data.Odbc", StringComparison.OrdinalIgnoreCase))
			{
				if (_odbc == null)
					lock (obj4Lock)
						_odbc = LoadFromAssembly("System.Data.Odbc", "System.Data.Odbc.OdbcFactory");
				return _odbc;
			}
			else
			{
				if (!_factories.TryGetValue(providerInvariantName, out DbProviderFactory factory))
				{
					factory = LoadFromAssembly(providerInvariantName);
					_factories[providerInvariantName] = factory;
				}
				return factory;
			}
		}

		private static DbProviderFactory LoadFromAssembly(string assemblyName, string typeFactoryName)
			=> LoadFromAssembly(assemblyName, t => t.FullName.StartsWith(typeFactoryName, StringComparison.OrdinalIgnoreCase));

		private static DbProviderFactory LoadFromAssembly(string assemblyName)
			=> LoadFromAssembly(assemblyName, t => typeof(DbProviderFactory).IsAssignableFrom(t));
		
		private static DbProviderFactory LoadFromAssembly(string assemblyName, Func<Type, bool> typesFilterDelegate)
			=> (DbProviderFactory)Assembly.Load(assemblyName)
				.GetTypes()
				.FirstOrDefault(t => typesFilterDelegate(t))
				.GetField("Instance", BindingFlags.GetProperty | BindingFlags.Static | BindingFlags.Public)
				.GetValue(null);
	}
}
