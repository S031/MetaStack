using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace S031.MetaStack.Core.Data
{
	public static class DbProviderFactories
	{
		/// <summary>
		/// Return instance of <see cref="System.Data.Common.DbProviderFactory"/> for specified provider name 
		/// </summary>
		/// <param name="providerInvariantName">Provider Name aka <see cref="System.Data.SqlClient"</param>
		/// <returns></returns>
		public static DbProviderFactory GetFactory(string providerInvariantName)
		{
			providerInvariantName.NullTest(nameof(providerInvariantName));

			var f = GetFactoryInternal(providerInvariantName);
			if (f != null)
				return f;

			App.ApplicationContext.Services.AddFromAssembly<DbProviderFactory>(ServiceLifetime.Singleton, (s, t) => t.CreateInstance());
			f = GetFactoryInternal(providerInvariantName);
			if (f != null)
				return f;

			//The type derived from DbProviderFactory was not found in {0}
			throw new InvalidOperationException("S031.MetaStack.Core.Data.DbProviderFactories.GetFactory.2"
				.GetTranslate(providerInvariantName));
		}

		private static DbProviderFactory GetFactoryInternal(string providerInvariantName)
		{
			return App.ApplicationContext.GetServices().GetServices<DbProviderFactory>()?
				.FirstOrDefault(p => p.GetType().FullName.StartsWith(providerInvariantName, StringComparison.CurrentCultureIgnoreCase));
		}

		/// <summary>
		/// Return list of Provider names from all loaded assemblies
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<string> GetFactoryProviderNames()
		{
			foreach (var f in App.ApplicationContext.GetServices().GetServices<DbProviderFactory>())
				yield return f.GetType().FullName;
		}
	}
}
