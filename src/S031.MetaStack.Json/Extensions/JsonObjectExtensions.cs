using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace S031.MetaStack.Json
{
	public static class JsonObjectExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool GetBoolOrDefault(this JsonObject json, string key)
			=> json.TryGetValue(key, out JsonValue value)
				&& value.JsonType == JsonType.Boolean 
				&& (bool)value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool GetBoolOrDefault(this JsonObject json, string key, bool defaultValue)
			=> json.TryGetValue(key, out JsonValue value)
				&& value.JsonType == JsonType.Boolean
				? (bool)value
				: defaultValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetIntOrDefault(this JsonObject json, string key)
			=> json.TryGetValue(key, out JsonValue value)
				&& value.JsonType == JsonType.Integer
				? (int)value
				: default;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetIntOrDefault(this JsonObject json, string key, int defaultValue)
			=> json.TryGetValue(key, out JsonValue value)
				&& value.JsonType == JsonType.Integer
				? (int)value
				: defaultValue;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long GetLongOrDefault(this JsonObject json, string key)
			=> json.TryGetValue(key, out JsonValue value)
				&& value.JsonType == JsonType.Integer
				? (long)value
				: default;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double GetDoubleOrDefault(this JsonObject json, string key)
			=> json.TryGetValue(key, out JsonValue value)
				&& value.JsonType == JsonType.Float
				? (double)value
				: default;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetStringOrDefault(this JsonObject json, string key)
			=> json.TryGetValue(key, out JsonValue value)
				&& value.JsonType == JsonType.String
				? (string)value
				: string.Empty;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetStringOrDefault(this JsonObject json, string key, string defaultValue)
			=> json.TryGetValue(key, out JsonValue value)
				&& value.JsonType == JsonType.String
				? (string)value
				: string.Empty;

		public static Guid GetGuidOrDefault(this JsonObject json, string key)
		{
			if (json.TryGetValue(key, out JsonValue value))
				if (value.JsonType == JsonType.Guid)
					return (Guid)value;
				else if (value.JsonType == JsonType.String
					&& Guid.TryParse(value, out Guid guid))
					return guid;
			return Guid.Empty;
		}

		public static Guid GetGuidOrDefault(this JsonObject json, string key, Func<Guid> defaultValueCreator)
		{
			if (json.TryGetValue(key, out JsonValue value))
				if (value.JsonType == JsonType.Guid)
					return (Guid)value;
				else if (value.JsonType == JsonType.String
					&& Guid.TryParse(value, out Guid guid))
					return guid;
			return defaultValueCreator();
		}

		public static DateTime GetDateOrDefault(this JsonObject json, string key)
		{
			if (json.TryGetValue(key, out JsonValue value))
				if (value.JsonType == JsonType.Date)
					return (DateTime)value;
				else if (value.JsonType == JsonType.String
					&& DateTime.TryParse(value, out DateTime date))
					return date;
			return default;
		}

		public static DateTime GetDateOrDefault(this JsonObject json, string key, DateTime defaultValue)
		{
			if (json.TryGetValue(key, out JsonValue value))
				if (value.JsonType == JsonType.Date)
					return (DateTime)value;
				else if (value.JsonType == JsonType.String
					&& DateTime.TryParse(value, out DateTime date))
					return date;
			return defaultValue;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetEnum<T>(this JsonObject json, string key) where T : struct
			=> json.TryGetValue(key, out JsonValue value)
				&& value.JsonType == JsonType.String
				&& Enum.TryParse<T>(value, out T result) ? result : default;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetEnum<T>(this JsonObject json, string key, T defaultValue) where T : struct
			=> json.TryGetValue(key, out JsonValue value)
				&& value.JsonType == JsonType.String
				&& Enum.TryParse<T>(value, out T result) ? result : defaultValue;
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static JsonArray GetArray(this JsonObject json, string key)
			=> json.TryGetValue(key, out JsonValue value)
				&& value.JsonType == JsonType.Array? (value as JsonArray) : null;

	}
}
