using S031.MetaStack.Common;
using System;

namespace S031.MetaStack.Buffers
{
	public static class BinaryDataFormatterService
	{
		private static readonly IBinaryDataFormatter _default = null;
		private static readonly MapTable<Type, IBinaryDataFormatter> _formatters = new MapTable<Type, IBinaryDataFormatter>();

		public static void Register<TFormatter, T>() where TFormatter : IBinaryDataFormatter, new()
		{
			var instance = (IBinaryDataFormatter)typeof(TFormatter).CreateInstance();
			_formatters[typeof(T)] = instance;
		}

		public static IBinaryDataFormatter Default
			=> _default;

		public static bool Resolve<T>(out IBinaryDataFormatter formatter)
			=> _formatters.TryGetValue(typeof(T), out formatter);

		public static bool Resolve(Type type, out IBinaryDataFormatter formatter)
			=> _formatters.TryGetValue(type, out formatter);

	}
}
