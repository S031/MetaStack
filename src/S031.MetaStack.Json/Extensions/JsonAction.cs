using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Json
{
	public struct JsonAction
	{
		public JsonAction(Type sourceType, 
			Action<JsonWriter, object> writeAction, 
			Action<JsonValue, object> readDelegate)
		{
			SourceType = sourceType;
			WriteDelegate = writeAction;
			ReadDelegate = readDelegate;
		}

		public Type SourceType { get; }

		public Action<JsonWriter, object> WriteDelegate { get; }

		public Action<JsonValue, object> ReadDelegate { get; }

	}
}
