using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace S031.MetaStack.Json
{
	public enum Formatting
	{
		None = 0,
		Indented = 1
	}
	public sealed class JsonWriter
	{
		private const int default_capacity = 512;

		private readonly StringBuilder _sb;
		private readonly Formatting _formatting;
		private bool _commaExpected;
		private int _tabCount;

		private static readonly ConcurrentDictionary<Type, Action<JsonWriter, object>> _wellKnownTypes =
			new ConcurrentDictionary<Type, Action<JsonWriter, object>>();

		public JsonWriter(Formatting formatting)
		{
			_sb = new StringBuilder(default_capacity);
			_formatting = formatting;
			_commaExpected = false;
			_tabCount = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public JsonWriter WriteChar(char value)
		{
			_sb.Append(value);
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public JsonWriter WriteStartArray()
		{
			if (_commaExpected)
				WriteComma();
			else
				WriteNewLine();
			WriteIndent();
			_tabCount++;
			_sb.Append('[');
			_commaExpected = false;
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public JsonWriter WriteEndArray()
		{
			_tabCount--;
			WriteNewLine();
			WriteIndent();
			_sb.Append(']');
			_commaExpected = true;
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public JsonWriter WriteStartObject()
		{
			if (_commaExpected)
				WriteComma();
			else
				WriteNewLine();
			WriteIndent();
			_tabCount++;
			_sb.Append('{');
			WriteNewLine();
			_commaExpected = false;
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public JsonWriter WriteEndObject()
		{
			_tabCount--;
			WriteNewLine();
			WriteIndent();
			_sb.Append('}');
			_commaExpected = true;
			return this;
		}

		public JsonWriter WritePropertyName(string propertyName)
			=> WritePropertyName(propertyName, false);

		public JsonWriter WritePropertyName(string propertyName, bool escaped)
		{
			if (_commaExpected)
				WriteComma();
			WriteIndent();
			_sb.Append('"');
			if (escaped)
				WriteEscapeString(ref propertyName);
			else
				_sb.Append(propertyName);
			_sb.Append('"');
			_sb.Append(':');
			if (_formatting == Formatting.Indented)
				_sb.Append(' ');

			_commaExpected = false;
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void WriteIndent()
		{
			if (_formatting == Formatting.Indented)
				for (int i = 0; i < _tabCount; i++)
					_sb.Append('\t');
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void WriteNewLine()
		{
			if (_formatting == Formatting.Indented)
				_sb.Append('\n');
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void WriteComma()
		{
			_sb.Append(',');
			WriteNewLine();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private JsonWriter WriteBoolean(bool value)
		{
			if (_commaExpected)
				WriteComma();
			_sb.Append(value ? "true" : "false");
			_commaExpected = true;
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private JsonWriter WriteNumber(long value)
		{
			if (_commaExpected)
				WriteComma();
			_sb.Append(value.ToString("g", JsonHelper.NumberFormatInfo));
			_commaExpected = true;
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private JsonWriter WriteNumber(int value)
		{
			if (_commaExpected)
				WriteComma();
			_sb.Append(value.ToString("g", JsonHelper.NumberFormatInfo));
			_commaExpected = true;
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private JsonWriter WriteNumber(decimal value)
		{
			if (_commaExpected)
				WriteComma();
			_sb.Append(value.ToString("g", JsonHelper.NumberFormatInfo));
			_commaExpected = true;
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private JsonWriter WriteNumber(double value)
		{
			if (_commaExpected)
				WriteComma();
			_sb.Append(value.ToString("g", JsonHelper.NumberFormatInfo));
			_commaExpected = true;
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public JsonWriter WriteNull()
		{
			if (_commaExpected)
				WriteComma();
			_sb.Append("null");
			_commaExpected = true;
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private JsonWriter WriteString(string value, bool escaped)
		{
			if (_commaExpected)
				WriteComma();
			_sb.Append('"');
			if (escaped)
				WriteEscapeString(ref value);
			else
				_sb.Append(value);
			_sb.Append('"');
			_commaExpected = true;
			return this;
		}

		public JsonWriter WriteValue(string value)
			=> WriteString(value, true);

		public JsonWriter WriteValue(TimeSpan value)
			=> WriteString(value.ToString("c"), false);

		public JsonWriter WriteValue(Guid value)
			=> WriteString(value.ToString(), false);
		
		public JsonWriter WriteValue(DateTimeOffset value)
			=> WriteString(value.ToString("c"), false);

		public JsonWriter WriteValue(DateTime value)
			=> WriteString(value.ToString("yyyy-MM-ddTHH:mm:ss.fff"), false);

		public JsonWriter WriteValue(decimal value)
			=> WriteNumber(value);

		public JsonWriter WriteValue(sbyte value)
			=> WriteNumber((int)value);

		public JsonWriter WriteValue(bool value)
			=> WriteBoolean(value);

		public JsonWriter WriteValue(byte value)
			=> WriteNumber((int)value);

		public JsonWriter WriteValue(int value)
			=> WriteNumber((int)value);

		public JsonWriter WriteValue(uint value)
			=> WriteNumber((long)value);

		public JsonWriter WriteValue(long value)
			=> WriteNumber((long)value);

		public JsonWriter WriteValue(ulong value)
			=> WriteNumber((long)value);

		public JsonWriter WriteValue(ushort value)
			=> WriteNumber((int)value);

		public JsonWriter WriteValue(short value)
			=> WriteNumber((int)value);

		public JsonWriter WriteValue(float value)
			=> WriteNumber(value);

		public JsonWriter WriteValue(double value)
			=> WriteNumber(value);

		public JsonWriter WriteValue(byte[] value)
			=> WriteString(Convert.ToBase64String(value), false);
			
		public JsonWriter WriteValue(char value)	
			=> WriteString(value.ToString(), false);
	
		public JsonWriter WriteValue(Uri value)
			=> WriteString(value.ToString(), false);

		public JsonWriter WriteRaw(string json)
		{
			_sb.Append(json);
			return this;
		}

		public JsonWriter WriteValue(JsonValue v)
		{
			switch (v.JsonType)
			{
				case JsonType.Object:
					((JsonObject)v).WriteRaw(this);
					_commaExpected = true;
					break;
				case JsonType.Array:
					((JsonArray)v).WriteRaw(this);
					_commaExpected = true;
					break;
				case JsonType.Null:
					WriteNull();
					break;
				case JsonType.String:
					WriteValue((string)v.Value);
					break;
				case JsonType.Float:
					WriteValue(Convert.ToDouble(v.Value));
					break;
				case JsonType.Integer:
					WriteValue(Convert.ToInt64(v.Value));
					break;
				case JsonType.Date:
					WriteValue((DateTime)v.Value);
					break;
				case JsonType.Boolean:
					WriteValue((bool)v.Value);
					break;
				case JsonType.Guid:
					WriteValue((Guid)v.Value);
					break;
				case JsonType.Uri:
					WriteValue((Uri)v.Value);
					break;
				case JsonType.Bytes:
					WriteValue((byte[])v.Value);
					break;
				case JsonType.TimeSpan:
					WriteValue((TimeSpan)v.Value);
					break;
				default:
					if (_wellKnownTypes.TryGetValue(v.Value.GetType(), out var f))
						f(this, v.Value);
					else
						//!!! see for enum
						WriteValue(v.Value.ToString());
					break;
			}
			return this;
		}

		public bool CommaExpected { get => _commaExpected; set => _commaExpected = value; }

		// Characters which have to be escaped:
		// - Required by JSON Spec: Control characters, '"' and '\\'
		// - Broken surrogates to make sure the JSON string is valid Unicode
		//   (and can be encoded as UTF8)
		// - JSON does not require U+2028 and U+2029 to be escaped, but
		//   JavaScript does require this:
		//   http://stackoverflow.com/questions/2965293/javascript-parse-error-on-u2028-unicode-character/9168133#9168133
		// - '/' also does not have to be escaped, but escaping it when
		//   preceeded by a '<' avoids problems with JSON in HTML <script> tags
		private static bool NeedEscape(string src, int i)
		{
			char c = src[i];
			return c < 32 || c == '"' || c == '\\'
				// Broken lead surrogate
				|| (c >= '\uD800' && c <= '\uDBFF' &&
					(i == src.Length - 1 || src[i + 1] < '\uDC00' || src[i + 1] > '\uDFFF'))
				// Broken tail surrogate
				|| (c >= '\uDC00' && c <= '\uDFFF' &&
					(i == 0 || src[i - 1] < '\uD800' || src[i - 1] > '\uDBFF'))
				// To produce valid JavaScript
				|| c == '\u2028' || c == '\u2029'
				// Escape "</" for <script> tags
				|| (c == '/' && i > 0 && src[i - 1] == '<');
		}

		internal void WriteEscapeString(ref string src)
		{
			if (src != null)
			{
				for (int i = 0; i < src.Length; i++)
				{
					if (NeedEscape(src, i))
					{
						if (i > 0)
							_sb.Append(src, 0, i);
						DoEscapeString(ref src, i);
						return;
					}
				}
				_sb.Append(src);
			}
		}

		private void DoEscapeString(ref string src, int cur)
		{
			int start = cur;
			var sb = _sb;
			for (int i = cur; i < src.Length; i++)
				if (NeedEscape(src, i))
				{
					sb.Append(src, start, i - start);
					switch (src[i])
					{
						case '\b': sb.Append("\\b"); break;
						case '\f': sb.Append("\\f"); break;
						case '\n': sb.Append("\\n"); break;
						case '\r': sb.Append("\\r"); break;
						case '\t': sb.Append("\\t"); break;
						case '\"': sb.Append("\\\""); break;
						case '\\': sb.Append("\\\\"); break;
						case '/': sb.Append("\\/"); break;
						default:
							sb.Append("\\u");
							sb.Append(((int)src[i]).ToString("x04"));
							break;
					}
					start = i + 1;
				}
			sb.Append(src, start, src.Length - start);
		}

		public override string ToString()
			=> _sb.ToString();

		public static void AddWellKnown(Type type, Action<JsonWriter, object> writeDelegate)
			=> _wellKnownTypes[type] = writeDelegate;
	}
}
