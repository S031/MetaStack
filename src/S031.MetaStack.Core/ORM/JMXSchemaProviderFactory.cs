using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using S031.MetaStack.Common;

#if NETCOREAPP
namespace S031.MetaStack.Core.ORM
#else
namespace S031.MetaStack.WinForms.ORM
#endif
{
	public static class JMXSchemaProviderFactory
	{

		private static IJMXSchemaProvider _default = null;

		public static IJMXSchemaProvider Default => _default;

		public static void RegisterProvider<T>() where T : class, IJMXSchemaProvider
		{
			ImplementsList.Add(typeof(IJMXSchemaProvider));
		}
		public static void SetDefault(IJMXSchemaProvider instance) =>
			_default = instance;

		public static T GetProvider<T>(params object[] ctorArgs) where T : class, IJMXSchemaProvider
		{
			return ImplementsList.GetTypes(typeof(IJMXSchemaProvider))?
				.FirstOrDefault(t => t.Equals(typeof(T)))?.CreateInstance<T>(ctorArgs);
		}

	}
}
