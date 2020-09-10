using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace S031.MetaStack.Common
{
	public static class ImplementsList
	{
		private static object obj4Lock = new object();
		private static readonly MapTable<Type, List<Type>> _iList = new MapTable<Type, List<Type>>();

		public static IEnumerable<Type> GetTypes(Type type)
		{
			if (_iList.TryGetValue(type, out var l))
				return l;

			l = GetImplements(type).ToList();
			_iList.TryAdd(type, l);
			return l;
		}

		public static IEnumerable<Type> Add(Type type, Assembly a = null)
		{
			var implements = GetImplements(type, a);
			if (_iList.ContainsKey(type))
			{
				var l = _iList[type];
				lock(obj4Lock)
					l.AddRange(implements.Where(t => !l.Contains(t)));
				return l;
			}
			else
			{
				var l = implements.ToList();
				_iList.TryAdd(type, l);
				return l;
			}
		}
        private static IEnumerable<Type> GetImplements(Type type, Assembly assembly = null)
        {
            IEnumerable<Assembly> l = assembly == null ? GetAssemblies() : new List<Assembly>() { assembly };
            foreach (var a in l)
#if NETCOREAPP //&& DEBUG
                if (!a.FullName.StartsWith("Microsoft.VisualStudio.TraceDataCollector", StringComparison.Ordinal))
#endif
                    foreach (Type t in a.GetTypes().Where(t => type.IsAssignableFrom(t) && !type.Equals(t)))
                        yield return t;
        }

		private static IEnumerable<Assembly> GetAssemblies() => AppDomain.CurrentDomain.GetAssemblies();
	}
}
