using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Json
{
	public struct JsonAction
	{
		public JsonAction(Type sourceType, 
			Action<JsonWriter, object> writeAction, 
			Action<JsonReader, object> readDelegate,
			Func<object> ctorDelegate)
		{
			SourceType = sourceType;
			WriteDelegate = writeAction;
			ReadDelegate = readDelegate;
			CtorDelegate = ctorDelegate;
		}

		public Type SourceType { get; }

		public Action<JsonWriter, object> WriteDelegate { get; }

		public Action<JsonReader, object> ReadDelegate { get; }

		public Func<object> CtorDelegate { get; }
	}
}
