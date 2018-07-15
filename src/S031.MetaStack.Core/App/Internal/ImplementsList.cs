using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
#if NETCOREAPP
using Microsoft.Extensions.DependencyModel;

namespace S031.MetaStack.Core
#else

namespace S031.MetaStack.WinForms
#endif
{
	public static class ImplementsList
	{
		static readonly object objLock = new object();

		static readonly Dictionary<Type, List<Type>> _iList = new Dictionary<Type, List<Type>>();

		static readonly IServiceCollection _services = new ServiceCollection();

		public static IServiceProvider GetServiceProvider() => _services.BuildServiceProvider();

		public static IServiceCollection GetServices() => _services;

		public static IEnumerable<Type> GetTypes(Type type)
		{
			if (_iList.TryGetValue(type, out var l))
				return l;
			return default(List<Type>);
		}

		public static IEnumerable<Type> Add(Type type, Assembly a = null)
		{
			if (_iList.ContainsKey(type))
			{
				lock (objLock)
				{
					var l = _iList[type];
					l.AddRange(getImplements(type, a).Where(t => !l.Contains(t)));
					return l;
				}
			}
			else
			{
				lock (objLock)
				{
					var l = getImplements(type, a).ToList();
					_iList.Add(type, l);
					return l;
				}
			}
		}
		private static IEnumerable<Type> getImplements(Type type, Assembly assembly = null)
		{
			IEnumerable<Assembly> l = assembly == null ? getAssemblies() : new List<Assembly>() { assembly };
			foreach (var a in l)
				foreach (Type t in a.GetTypes().Where(t => type.IsAssignableFrom(t) && !type.Equals(t)))
					yield return t;
		}
		private static IEnumerable<Assembly> getAssemblies()
		{
#if NETCOREAPP
			foreach (var a in DependencyContext.Default.CompileLibraries
				.Select<CompilationLibrary, Assembly>(l => TryLoad(l.Name)).Where(ass=>ass != null))
				yield return a;
#else
			return AppDomain.CurrentDomain.GetAssemblies();
#endif
		}
		private static Assembly TryLoad(string name)
		{
			try
			{
				return Assembly.Load(new AssemblyName(name));
			}
			catch { }
			return null;
		}
	}
}
