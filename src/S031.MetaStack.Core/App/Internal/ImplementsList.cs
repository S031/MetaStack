using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if NETCOREAPP

namespace S031.MetaStack.Core
#else

namespace S031.MetaStack.WinForms
#endif
{
	public static class ImplementsList
	{
		static readonly MapTable<Type, List<Type>> _iList = new MapTable<Type, List<Type>>();

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
			if (_iList.ContainsKey(type))
			{
				var l = _iList[type];
				l.AddRange(GetImplements(type, a).Where(t => !l.Contains(t)));
				return l;
			}
			else
			{
				var l = GetImplements(type, a).ToList();
				_iList.Add(type, l);
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
