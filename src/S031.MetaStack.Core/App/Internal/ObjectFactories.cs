using Microsoft.Extensions.DependencyInjection;
using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S031.MetaStack.Core
{
	public static class ObjectFactories
	{
		private static readonly object _obj4Lock = new object();
		private static readonly Dictionary<Type, object> _objectDefaultsCache = new Dictionary<Type, object>();

		public static T GetFactory<T>(string invariantName)
		{
			return GetFactory<T>(p => p.GetType()
				.FullName
				.StartsWith(invariantName, StringComparison.CurrentCultureIgnoreCase));
		}

		public static T GetFactory<T>(Func<T, bool> filter)
		{
			T f = GetFactoryInternal<T>(filter);
			if (f != null)
				return f;

			App.ApplicationContext.Services.AddFromAssembly<T>(ServiceLifetime.Singleton, (s, t) => t.CreateInstance());
			f = GetFactoryInternal<T>(filter);
			if (f != null)
				return f;

			//The type derived from {0} was not found in service collection
			throw new InvalidOperationException("S031.MetaStack.Core.Data.ObjectFactories.GetFactories.1"
				.GetTranslate(typeof(T).FullName));

		}

		private static T GetFactoryInternal<T>(Func<T, bool> filter)
		{
			return App.ApplicationContext
				.GetServices()
				.GetServices<T>()
				.FirstOrDefault(p => filter(p));
		}
		
		public static void SetDefault<T>(T instance)
		{
			lock (_obj4Lock)
				_objectDefaultsCache[typeof(T)] = instance;
		}

		public static T GetDefault<T>()
		{
			if (_objectDefaultsCache.TryGetValue(typeof(T), out object instance))
				return (T)instance;
			T result = GetFactoryInternal<T>(p => true);
			if (result != null)
			{
				SetDefault<T>(result);
				return result;
			}
			return default;		
		}
		
		/// <summary>
		/// Return list of factories names from all loaded assemblies
		/// Take effect when runing after <see cref="GetFactory{T}(Func{T, bool})"/>
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<string> GetFactoryNames<T>()
		{
			foreach (var f in App.ApplicationContext.GetServices().GetServices<T>())
				yield return f.GetType().FullName;
		}
	}
}
