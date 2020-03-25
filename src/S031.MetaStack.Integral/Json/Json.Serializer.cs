﻿using S031.MetaStack.Common;
using System;

namespace S031.MetaStack.Json
{
	public static partial class JsonExtensions
	{
		public static object DeserializeObject(Type t, string value)
		{
			if (typeof(JsonSerializible).IsAssignableFrom(t))
			{
				JsonValue jsonValue = new JsonReader(value).Read();
				return t.CreateInstance(jsonValue);
			}
			else if (JsonWellKnownTypes.TryGetValue(t, out var f))
			{
				JsonValue jsonValue = new JsonReader(value).Read();
				var instance = t.CreateInstance();
				f.ReadDelegate(jsonValue, instance);
				return instance;

			}
			throw new NotImplementedException();
		}

		public static T DeserializeObject<T>(string value)
		{
			Type t = typeof(T);
			if (typeof(JsonSerializible).IsAssignableFrom(t))
			{
				JsonValue jsonValue = new JsonReader(value).Read();
				return t.CreateInstance<T>(jsonValue);
			}
			else if (JsonWellKnownTypes.TryGetValue(t, out var f))
			{
				JsonValue jsonValue = new JsonReader(value).Read();
				var instance = t.CreateInstance();
				f.ReadDelegate(jsonValue, instance);
				return (T)instance;

			}
			throw new NotImplementedException();
		}

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
			throw new NotImplementedException();
		}
	}
}