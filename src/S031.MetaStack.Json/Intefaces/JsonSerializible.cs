using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Json
{
	public abstract class JsonSerializible : IJsonSerializible
	{
		public JsonSerializible(JsonValue source)
		{
			if (source != null)
				FromJson(source);
		}

		public override string ToString()
			=> ToString(Formatting.None);

		public virtual string ToString(Formatting formatting)
		{
			JsonWriter writer = new JsonWriter(formatting);
			ToJson(writer);
			return writer.ToString();
		}

		public virtual void ToJson(JsonWriter writer)
		{
			writer.WriteStartObject();
			ToJsonRaw(writer);
			writer.WriteEndObject();
		}

		protected abstract void ToJsonRaw(JsonWriter writer);

		public abstract void FromJson(JsonValue source);

	}
}
