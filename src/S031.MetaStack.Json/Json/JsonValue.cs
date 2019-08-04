using System;
using System.Collections;
using System.IO;
using System.Text;
using JsonPair = System.Collections.Generic.KeyValuePair<string, S031.MetaStack.Json.JsonValue>;

namespace S031.MetaStack.Json
{
	public partial class JsonValue : IEnumerable
	{
		private static readonly UTF8Encoding s_encoding = new UTF8Encoding(false, true);

		public static JsonValue Parse(string jsonString)
		{
			if (jsonString == null)
			{
				throw new ArgumentNullException(nameof(jsonString));
			}

			return new JsonReader(ref jsonString).Read();
		}

		public virtual int Count => throw new InvalidOperationException();

		public virtual JsonValue this[int index]
		{
			get => throw new InvalidOperationException();
			set => throw new InvalidOperationException();
		}

		public virtual JsonValue this[string key]
		{
			get => throw new InvalidOperationException();
			set => throw new InvalidOperationException();
		}

		public virtual bool ContainsKey(string key)
		{
			throw new InvalidOperationException();
		}

		public virtual void Save(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			using (StreamWriter writer = new StreamWriter(stream, s_encoding, 1024, true))
			{
				Save(writer);
			}
		}

		public virtual void Save(TextWriter textWriter)
		{
			if (textWriter == null)
			{
				throw new ArgumentNullException(nameof(textWriter));
			}

			SaveInternal(textWriter);
		}

		private void SaveInternal(TextWriter w)
		{
			switch (JsonType)
			{
				case JsonType.Object:
					w.Write('{');
					bool following = false;
					foreach (JsonPair pair in ((JsonObject)this))
					{
						if (following)
						{
							w.Write(", ");
						}
						w.Write('\"');
						//w.Write(EscapeString(pair.Key));
						w.Write(pair.Key);
						w.Write("\": ");
						if (pair.Value == null)
						{
							w.Write("null");
						}
						else
						{
							pair.Value.SaveInternal(w);
						}

						following = true;
					}
					w.Write('}');
					break;

				case JsonType.Array:
					w.Write('[');
					following = false;
					foreach (JsonValue v in ((JsonArray)this))
					{
						if (following)
						{
							w.Write(", ");
						}

						if (v != null)
						{
							v.SaveInternal(w);
						}
						else
						{
							w.Write("null");
						}

						following = true;
					}
					w.Write(']');
					break;

				case JsonType.Boolean:
					w.Write(this ? "true" : "false");
					break;

				case JsonType.String:
					w.Write('"');
					w.Write(EscapeString((string)_value));
					w.Write('"');
					break;

				default:
					w.Write(this.GetFormattedString());
					break;
			}
		}

		public override string ToString()
		{
			using (StringWriter sw = new StringWriter())
			{
				SaveInternal(sw);
				return sw.ToString();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new InvalidOperationException();
		}

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

		internal static string EscapeString(string src)
		{
			if (src != null)
			{
				for (int i = 0; i < src.Length; i++)
				{
					if (NeedEscape(src, i))
					{
						StringBuilder sb = new StringBuilder(src.Length);
						if (i > 0)
						{
							sb.Append(src, 0, i);
						}
						return DoEscapeString(sb, src, i);
					}
				}
			}

			return src;
		}

		private static string DoEscapeString(StringBuilder sb, string src, int cur)
		{
			int start = cur;
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
			return sb.ToString();
		}

	}
}