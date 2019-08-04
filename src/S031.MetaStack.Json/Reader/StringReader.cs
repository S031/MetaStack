#if SPANJSON
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Json;
using SpanJson;
using JsonPair = System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>;

namespace S031.MetaStack.Json
{
	public static partial class Extensions
	{
		public static JsonValue ToJsonValue(this string source)
		{
			return JsonSpanReader.Read(ref source);
		}

	}
	internal static class JsonSpanReader
	{
		public static JsonValue Read(ref string source)
		{
			var json = new JsonReader<char>(source);
			return ToJsonValue(ref json);
		}

		private static JsonValue ToJsonValue(ref JsonReader<char> json)
		{

			JsonToken tokenType;
			while ((tokenType = json.ReadUtf16NextToken()) != JsonToken.None)
			{

				switch (tokenType)
				{
					case JsonToken.BeginObject:
						{
							json.SkipNextUtf16Value(tokenType);
							TryGetObject(ref json, out JsonValue value);
							return value;
						}
					case JsonToken.BeginArray:
						{
							var list = new List<JsonValue>();
							json.SkipNextUtf16Value(tokenType);
							while ((tokenType = json.ReadUtf16NextToken()) != JsonToken.None)
							{
								if (tokenType == JsonToken.EndArray)
								{
									break;
								}
								else if (TryGetValue(ref json, tokenType, out JsonValue value))
								{
									list.Add(value);
								}
								else if (TryGetObject(ref json, out value))
								{
									list.Add(value);
								}
							}
							return new JsonArray(list);
						}
					case JsonToken.EndObject:
					case JsonToken.EndArray: 
					case JsonToken.Null:
					case JsonToken.NameSeparator:
						json.SkipNextUtf16Value(tokenType);
						break;
					case JsonToken.String:
						return json.ReadUtf16String();
					case JsonToken.Number:
						{
							TryGetNumber(ref json, out JsonValue value);
							return value;
						}
					case JsonToken.True:
					case JsonToken.False:
						return json.ReadUtf16Boolean();
					default:
						throw new ArgumentException();
				}
			}
			return new JsonObject();
		}

		private static bool TryGetObject(ref JsonReader<char> json, out JsonValue value)
		{
			List<JsonPair> list = new List<JsonPair>();
			JsonToken tokenType;
			while ((tokenType = json.ReadUtf16NextToken()) != JsonToken.None)
			{
				if (tokenType == JsonToken.EndObject)
				{
					json.SkipNextUtf16Value(tokenType);
					break;
				}
				else if (tokenType == JsonToken.String)
				{
					list.Add(new JsonPair(json.ReadUtf16String(), ToJsonValue(ref json)));
				}
				else
					json.SkipNextUtf16Value(tokenType);

			}
			value = new JsonObject(list);
			return list.Count > 0;
		}

		private static bool TryGetValue(ref JsonReader<char> json, JsonToken tokenType, out JsonValue value)
		{
			if (tokenType == JsonToken.String)
			{
				string buff = json.ReadUtf16String();
				if (DateTime.TryParse(buff, out DateTime dateTimeValue))
					value = dateTimeValue;
				else
					value = buff;
				return true;
			}
			else if (tokenType == JsonToken.Number)
			{
				return TryGetNumber(ref json, out value);
			}
			else if (tokenType == JsonToken.False
				|| tokenType == JsonToken.True)
			{
				value = json.ReadUtf16Boolean();
				return true;
			}
			else if (tokenType == JsonToken.Null)
			{
				value = null;
				return true;
			}
			value = null;
			return false;
		}

		private static bool TryGetNumber(ref JsonReader<char> json, out JsonValue value)
		{
			try
			{
				value = json.ReadUtf16Int32();
				return true;
			}
			catch { }

			try
			{
				value = json.ReadUtf16Int64();
				return true;
			}
			catch { }

			try
			{
				value = json.ReadUtf16Double();
				return true;
			}
			catch { }
			value = json.ReadUtf16Decimal();
			return true;
		}
	}
}
#endif