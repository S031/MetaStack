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
		private static readonly MapTable<string, DbProviderFactory> _factories = new MapTable<string, DbProviderFactory>(StringComparer.OrdinalIgnoreCase);

		private class ProviderInfo
		{
			public string AssemblyName;
			public string TypeFactoryName;
		}

		//fast cache for several types
		private static readonly IReadOnlyDictionary<string, ProviderInfo> _providersCache = 
			new ReadOnlyCache<string, ProviderInfo>(StringComparer.OrdinalIgnoreCase,
			("System.Data.SqlClient", new ProviderInfo() { AssemblyName = "System.Data.SqlClient", TypeFactoryName = "System.Data.SqlClient.SqlClientFactory" }),
			("System.Data.SQLite", new ProviderInfo() { AssemblyName = "System.Data.SQLite", TypeFactoryName = "System.Data.SQLite.SQLiteFactory" }),
			("Sybase.Data.AseClient", new ProviderInfo() { AssemblyName = "Sybase.AdoNet4.AseClient", TypeFactoryName = "Sybase.Data.AseClient.AseClientFactory" }),
			("System.Data.Odbc", new ProviderInfo() { AssemblyName = "System.Data.Odbc", TypeFactoryName = "System.Data.Odbc.OdbcFactory" })
		);

		/// <summary>
		/// Return instance of <see cref="System.Data.Common.DbProviderFactory"/> for specified provider name 
		/// </summary>
		/// <param name="providerInvariantName">Provider Name aka <see cref="System.Data.SqlClient"</param>
		/// <returns></returns>
		public static DbProviderFactory GetFactory(string providerInvariantName)
		{
			if (!_factories.TryGetValue(providerInvariantName, out DbProviderFactory factory))
			{
				if (_providersCache.TryGetValue(providerInvariantName, out ProviderInfo pi))
				{
					factory = LoadFromAssembly(pi.AssemblyName, pi.TypeFactoryName);
					_factories[providerInvariantName] = factory;
				}
				else
				{
					factory = LoadFromAssembly(providerInvariantName);
					_factories[providerInvariantName] = factory;
				}
			}
			return factory;
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
