using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Json
{
	public static class JsonObjectExtensions
	{
		public static bool GetBoolOrDefault(this JsonObject json, string key)
		{
			if (json.ContainsKey(key)
				&& json[key].JsonType == JsonType.Boolean)
				return (bool)json[key];
			return false;
		}

		public static int GetIntOrDefault(this JsonObject json, string key)
		{
			if (json.ContainsKey(key)
				&& json[key].JsonType == JsonType.Integer)
				return (int)json[key];
			return 0;
		}

		public static long GetLongOrDefault(this JsonObject json, string key)
		{
			if (json.ContainsKey(key)
				&& json[key].JsonType == JsonType.Integer)
				return (long)json[key];
			return 0L;
		}

		public static double GetDoubleOrDefault(this JsonObject json, string key)
		{
			if (json.ContainsKey(key)
				&& json[key].JsonType == JsonType.Float)
				return (double)json[key];
			return 0D;
		}

		public static string GetStringOrDefault(this JsonObject json, string key)
		{
			if (json.ContainsKey(key)
				&& json[key].JsonType == JsonType.String)
				return (string)json[key];
			return string.Empty;
		}

		public static Guid GetGuidOrDefault(this JsonObject json, string key)
		{
			if (json.ContainsKey(key)
				&& (json[key].JsonType == JsonType.Guid || json[key].JsonType == JsonType.String)
				&& Guid.TryParse((string)json[key], out Guid guid))
				return guid;
			return default;
		}

		public static DateTime GetDateOrDefault(this JsonObject json, string key)
		{
			if (json.ContainsKey(key)
				&& (json[key].JsonType == JsonType.Date || json[key].JsonType == JsonType.String)
				&& DateTime.TryParse((string)json[key], out DateTime d))
				return d;
			return default;
		}

	}
}
