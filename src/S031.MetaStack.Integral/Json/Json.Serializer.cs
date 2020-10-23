using S031.MetaStack.Common;
using System;
#if NETCOREAPP
using System.Text.Encodings.Web;
using System.Text.Json;
#endif

namespace S031.MetaStack.Json
{
	public static partial class JsonSerializer
	{
#if NETCOREAPP
		private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions() //net core 5 use JsonSerializerDefaults.Web
		{
			//IncludeFields = true, // NEW: globally include fields for (de)serialization
			//Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
			WriteIndented = false,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		};
#endif
		public static object DeserializeObject(Type t, string value)
		{
			if (typeof(JsonSerializible).IsAssignableFrom(t))
			{
				JsonValue jsonValue = new JsonReader(value).Read();
				return t.CreateInstance(jsonValue);
			}
			else if (typeof(IJsonSerializible).IsAssignableFrom(t))
			{
				JsonValue jsonValue = new JsonReader(value).Read();
				IJsonSerializible instance = (IJsonSerializible)t.CreateInstance(jsonValue);
				instance.FromJson(jsonValue);
				return instance;
			}
			else if (JsonWellKnownTypes.TryGetValue(t, out var f))
			{
				JsonValue jsonValue = new JsonReader(value).Read();
				var instance = t.CreateInstance();
				f.ReadDelegate(jsonValue, instance);
				return instance;

			}
#if NETCOREAPP
			else
				return System.Text.Json.JsonSerializer.Deserialize(value, t, _serializerOptions);
#else
			throw new NotImplementedException();
#endif
		}

		public static T DeserializeObject<T>(string value)
		{
			Type t = typeof(T);
			if (typeof(JsonSerializible).IsAssignableFrom(t))
			{
				JsonValue jsonValue = new JsonReader(value).Read();
				return t.CreateInstance<T>(jsonValue);
			}
			else if (typeof(IJsonSerializible).IsAssignableFrom(t))
			{
				JsonValue jsonValue = new JsonReader(value).Read();
				IJsonSerializible instance = (IJsonSerializible)t.CreateInstance<T>(jsonValue);
				instance.FromJson(jsonValue);
				return (T)instance;
			}
			else if (JsonWellKnownTypes.TryGetValue(t, out var f))
			{
				JsonValue jsonValue = new JsonReader(value).Read();
				var instance = t.CreateInstance();
				f.ReadDelegate(jsonValue, instance);
				return (T)instance;

			}
#if NETCOREAPP
			else
				return System.Text.Json.JsonSerializer.Deserialize<T>(value, _serializerOptions);
#else
			throw new NotImplementedException();
#endif				
		}

		public static string SerializeObject(object value, Formatting formatting = Formatting.None)
		{
			if (value is IJsonSerializible js)
				return js.ToString(formatting);
			else if (JsonWellKnownTypes.TryGetValue(value.GetType(), out var f))
			{
				JsonWriter writer = new JsonWriter(formatting);
				f.WriteDelegate(writer, value);
				return writer.ToString();
			}
#if NETCOREAPP
			else
				return System.Text.Json.JsonSerializer.Serialize(value, _serializerOptions);
#else
			throw new NotImplementedException();
#endif
		}
	}
}
