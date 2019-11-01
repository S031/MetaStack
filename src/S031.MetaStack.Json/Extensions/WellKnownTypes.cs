using S031.MetaStack.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Json
{
	public static class JsonWellKnownTypes
	{
		private static readonly MapTable<Type, JsonAction> _wellKnownTypes =
			new MapTable<Type, JsonAction>();

		public static void Register(JsonAction jsonAction)
			=> _wellKnownTypes[jsonAction.SourceType] = jsonAction;

		public static bool TryGetValue(Type sourceType, out JsonAction jsonAction)
			=> _wellKnownTypes.TryGetValue(sourceType, out jsonAction);
	}
}
