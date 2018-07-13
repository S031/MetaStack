using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace S031.MetaStack.Core.Data
{
	public static class DbProviderFactoriesOld
	{
		static readonly object _objForLock = new object();
		static readonly Dictionary<string, DbProviderFactory> _factories = new Dictionary<string, DbProviderFactory>(StringComparer.CurrentCultureIgnoreCase);
		
		/// <summary>
		/// Return instance of <see cref="System.Data.Common.DbProviderFactory"/> for specified provider name 
		/// </summary>
		/// <param name="providerInvariantName">Provider Name aka <see cref="System.Data.SqlClient"</param>
		/// <returns></returns>
		public static DbProviderFactory GetFactory(string providerInvariantName)
		{
			providerInvariantName.NullTest(nameof(providerInvariantName));
			if (_factories.TryGetValue(providerInvariantName, out DbProviderFactory f))
				return f;

			f = (DbProviderFactory)ImplementsList.GetTypes(typeof(DbProviderFactory))?
				.FirstOrDefault(t => t.FullName.StartsWith(providerInvariantName, StringComparison.CurrentCultureIgnoreCase))?
				.CreateInstance();
			if (f != null)
			{
				lock (_objForLock)
					_factories[providerInvariantName] = f;
				return f;
			}

			ImplementsList.Add(typeof(DbProviderFactory));
			f = (DbProviderFactory)ImplementsList.GetTypes(typeof(DbProviderFactory))?
				.FirstOrDefault(t => t.FullName.StartsWith(providerInvariantName, StringComparison.CurrentCultureIgnoreCase))?
				.CreateInstance();
			if (f != null)
			{
				lock (_objForLock)
					_factories[providerInvariantName] = f;
				return f;
			}
			//The type derived from DbProviderFactory was not found in {0}
			throw new InvalidOperationException("S031.MetaStack.Core.Data.DbProviderFactories.GetFactory.2"
				.GetTranslate(providerInvariantName));
		}

		/// <summary>
		/// Return list of Provider names from all loaded assemblies
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<string> GetFactoryProviderNames()
		{
			return ImplementsList.GetTypes(typeof(DbProviderFactory)).Select(t => t.FullName);
		}
	}
}
