using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Json
{
    public abstract class JsonSerializible
    {

        public JsonSerializible(JsonValue source) { }

        public virtual string ToString(Formatting formatting = Formatting.None)
        {
            JsonWriter writer = new JsonWriter(formatting);
            ToJson(writer);
            return writer.ToString();
        }

        public abstract void ToJson(JsonWriter writer);

        protected abstract void ToJsonRaw(JsonWriter writer);

    }
}
