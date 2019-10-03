using System;
using MessagePack;
using S031.MetaStack.Common;
using S031.MetaStack.Json;

#if NETCOREAPP
namespace S031.MetaStack.Core.Json
#else
namespace S031.MetaStack.WinForms.Json
#endif
{
	public static class JSONExtensions
    {

		public static object DeserializeObject(Type t, string value)
		{
			if (typeof(JsonSerializible).IsAssignableFrom(t))
			{
				JsonValue jsonValue = new JsonReader(ref value).Read();
				return t.CreateInstance(jsonValue);
			}
			else if (JsonWellKnownTypes.TryGetValue(t, out var f))
			{
				JsonValue jsonValue = new JsonReader(ref value).Read();
				var instance = t.CreateInstance();
				f.ReadDelegate(jsonValue, instance);
				return instance;

			}
			return MessagePackSerializer.Typeless.Deserialize(MessagePack.MessagePackSerializer.FromJson(value));
		}

		public static T DeserializeObject<T>(string value)
			=> (T)DeserializeObject(typeof(T), value);

		public static string SerializeObject(object value)
		{
			if (value is JsonSerializible js)
				return js.ToString(Formatting.None);
			else if (JsonWellKnownTypes.TryGetValue(value.GetType(), out var f))
			{
				JsonWriter writer = new JsonWriter(Formatting.None);
				f.WriteDelegate(writer, value);
				return writer.ToString();
			}
			return MessagePack.MessagePackSerializer.ToJson(MessagePackSerializer.Typeless.Serialize(value));
		}
	}
}
