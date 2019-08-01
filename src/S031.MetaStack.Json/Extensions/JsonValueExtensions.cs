using System.Collections.Generic;
using System.Diagnostics;

namespace S031.MetaStack.Json.Extensions
{
	internal class JsonValueExtensions
	{
		private static IEnumerable<KeyValuePair<string, JsonValue>> ToJsonPairEnumerable(IEnumerable<KeyValuePair<string, object>> kvpc)
		{
			foreach (KeyValuePair<string, object> kvp in kvpc)
			{
				yield return new KeyValuePair<string, JsonValue>(kvp.Key, ToJsonValue(kvp.Value));
			}
		}

		private static IEnumerable<JsonValue> ToJsonValueEnumerable(IEnumerable<object> arr)
		{
			foreach (object obj in arr)
			{
				yield return ToJsonValue(obj);
			}
		}

		public static JsonValue ToJsonValue(object ret)
		{
			if (ret == null)
			{
				return null;
			}

			if (ret is IEnumerable<KeyValuePair<string, object>> kvpc)
			{
				return new JsonObject(ToJsonPairEnumerable(kvpc));
			}

			if (ret is IEnumerable<object> arr)
			{
				return new JsonArray(ToJsonValueEnumerable(arr));
			}

			if (ret is bool) return new JsonValue((bool)ret);
			if (ret is decimal) return new JsonValue((decimal)ret);
			if (ret is double) return new JsonValue((double)ret);
			if (ret is int) return new JsonValue((int)ret);
			if (ret is long) return new JsonValue((long)ret);
			if (ret is string) return new JsonValue((string)ret);

			Debug.Assert(ret is ulong);
			return new JsonValue((ulong)ret);
		}
	}
}
