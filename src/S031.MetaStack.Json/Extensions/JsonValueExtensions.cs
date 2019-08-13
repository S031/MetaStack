using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace S031.MetaStack.Json
{
	internal static class JsonHelper
	{
		public static NumberFormatInfo NumberFormatInfo { get; } = new CultureInfo("en-US").NumberFormat;

		private static IEnumerable<KeyValuePair<string, JsonValue>> ToJsonPairEnumerable(IEnumerable<KeyValuePair<string, object>> kvpc)
		{
			foreach (KeyValuePair<string, object> kvp in kvpc)
				yield return new KeyValuePair<string, JsonValue>(kvp.Key, ToJsonValue(kvp.Value));
		}

		private static IEnumerable<JsonValue> ToJsonValueEnumerable(IEnumerable<object> arr)
		{
			foreach (object obj in arr)
				yield return ToJsonValue(obj);
		}

		public static JsonValue ToJsonValue(object ret)
		{
			if (ret == null)
				return new JsonValue();

			if (ret is JsonValue)
				return (JsonValue)ret;
			
			if (ret is IEnumerable<KeyValuePair<string, object>> kvpc)
				return new JsonObject(ToJsonPairEnumerable(kvpc));

			if (ret is IEnumerable<object> arr)
				return new JsonArray(ToJsonValueEnumerable(arr));

			return new JsonValue(ret);
		}
	}
}
