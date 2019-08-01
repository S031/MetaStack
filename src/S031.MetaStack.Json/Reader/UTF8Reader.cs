#if SPANJSON  
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Json;
using SpanJson;
using JsonPair = System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>;

namespace S031.MetaStack.Json
{
	public static partial class Extensions
	{
		public static JsonValue ToJsonValue(this byte[] source)
		{
			return JsonUTF8Reader.Read(ref source);
		}
	}
	internal static class JsonUTF8Reader
	{
		public static JsonValue Read(ref byte[] source)
		{
			var json = new JsonReader<byte>(source);
			return ToJsonValue(ref json);
		}

		private static JsonValue ToJsonValue(ref JsonReader<byte> json)
		{

			JsonToken tokenType;
			while ((tokenType = json.ReadUtf8NextToken()) != JsonToken.None)
			{

				switch (tokenType)
				{
					case JsonToken.BeginObject:
						{
							json.SkipNextUtf8Value(tokenType);
							TryGetObject(ref json, out JsonValue value);
							return value;
						}
					case JsonToken.BeginArray:
						{
							var list = new List<JsonValue>();
							json.SkipNextUtf8Value(tokenType);
							while ((tokenType = json.ReadUtf8NextToken()) != JsonToken.None)
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
						json.SkipNextUtf8Value(tokenType);
						break;
					case JsonToken.String:
						return json.ReadUtf8String();
					case JsonToken.Number:
						{
							TryGetNumber(ref json, out JsonValue value);
							return value;
						}
					case JsonToken.True:
					case JsonToken.False:
						return json.ReadBoolean();
					default:
						throw new ArgumentException();
				}
			}
			return new JsonObject();
		}

		private static bool TryGetObject(ref JsonReader<byte> json, out JsonValue value)
		{
			List<JsonPair> list = new List<JsonPair>();
			JsonToken tokenType;
			while ((tokenType = json.ReadUtf8NextToken()) != JsonToken.None)
			{
				if (tokenType == JsonToken.EndObject)
				{
					json.SkipNextUtf8Value(tokenType);
					break;
				}
				else if (tokenType == JsonToken.String)
				{
					list.Add(new JsonPair(json.ReadUtf8String(), ToJsonValue(ref json)));
				}
				else
					json.SkipNextUtf8Value(tokenType);

			}
			value = new JsonObject(list);
			return list.Count > 0;
		}

		private static bool TryGetValue(ref JsonReader<byte> json, JsonToken tokenType, out JsonValue value)
		{
			if (tokenType == JsonToken.String)
			{
				value = json.ReadUtf8String();
				return true;
			}
			else if (tokenType == JsonToken.Number)
			{
				return TryGetNumber(ref json, out value);
			}
			else if (tokenType == JsonToken.False
				|| tokenType == JsonToken.True)
			{
				value = json.ReadUtf8Boolean();
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

		private static bool TryGetNumber(ref JsonReader<byte> json, out JsonValue value)
		{
			var n = json.ReadNumberSpan();
			if (Utf8Parser.TryParse(n, out long longValue, out var consumed) && n.Length == consumed)
				value = longValue;
			else if (Utf8Parser.TryParse(n, out double doubleValue, out consumed) && n.Length == consumed)
				value = doubleValue;
			else if (Utf8Parser.TryParse(n, out decimal decimalValue, out consumed) && n.Length == consumed)
				value = decimalValue;
			else
				throw new NotFiniteNumberException();
			return true;
			//try
			//{
			//	value = json.ReadUtf8Int32();
			//	return true;
			//}
			//catch { }

			//try
			//{
			//	value = json.ReadUtf8Int64();
			//	return true;
			//}
			//catch { }

			//try
			//{
			//	value = json.ReadUtf8Double();
			//	return true;
			//}
			//catch { }

			//value = json.ReadUtf8Decimal();
			//return true;
		}
	}
}
#endif