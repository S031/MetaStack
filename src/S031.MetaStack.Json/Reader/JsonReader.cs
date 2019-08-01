using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using SR = S031.MetaStack.Json.Resources.Strings;

namespace S031.MetaStack.Json
{

	/// <summary>
	/// From source 
	/// https://github.com/dotnet/corefx/blob/master/src/System.Json/src/System/Json/JavaScriptReader.cs
	/// See changes after 2019-08-01
	/// </summary>
	public ref struct JsonReader
	{
		private const int default_capacity = 64;

		//private static readonly ThreadLocal<StringBuilder> _sb = new ThreadLocal<StringBuilder>(() => new StringBuilder(default_capacity));
		[ThreadStatic]
		private static readonly StringBuilder _sb = new StringBuilder(default_capacity);

		private readonly string _r;
		private readonly int _len;
#if DEBUG
		private int _line;
		private int _column;
		private bool _prev_lf;
#endif
		private int _peek;
		private int _cur;
		private bool _has_peek;

		public JsonReader(ref string source)
		{
			Debug.Assert(source != null);
#if DEBUG
			_line = 1;
			_column = 1;
			_prev_lf = false;
#endif
			_peek = 0;
			_has_peek = false;
			_cur = 0;
			_len = source.Length;
			_r = source;
		}

		public JsonValue Read()
		{
			JsonValue v = ReadCore();
			SkipSpaces();
			if (ReadChar() >= 0)
			{
				throw JsonError(SR.ArgumentException_ExtraCharacters);
			}
			return v;
		}

		private JsonValue ReadCore()
		{
			SkipSpaces();
			int c = PeekChar();
			if (c < 0)
			{
				throw JsonError(SR.ArgumentException_IncompleteInput);
			}

			switch (c)
			{
				case '[':
					ReadChar();
					JsonArray list = new JsonArray();
					SkipSpaces();
					if (PeekChar() == ']')
					{
						ReadChar();
						return list;
					}

					while (true)
					{
						list.Add(ReadCore());
						SkipSpaces();
						c = PeekChar();
						if (c != ',')
							break;
						ReadChar();
						continue;
					}

					if (ReadChar() != ']')
					{
						throw JsonError(SR.ArgumentException_ArrayMustEndWithBracket);
					}

					return list;

				case '{':
					ReadChar();
					JsonObject obj = new JsonObject();
					SkipSpaces();
					if (PeekChar() == '}')
					{
						ReadChar();
						return obj;
					}

					while (true)
					{
						SkipSpaces();
						if (PeekChar() == '}')
						{
							ReadChar();
							break;
						}
						string name = ReadStringLiteral();
						SkipSpaces();
						Expect(':');
						SkipSpaces();
						obj[name] = ReadCore(); // it does not reject duplicate names.
						SkipSpaces();
						c = ReadChar();
						if (c == ',')
						{
							continue;
						}
						if (c == '}')
						{
							break;
						}
					}
					return obj;

				case 't':
					Expect("true");
					return true;

				case 'f':
					Expect("false");
					return false;

				case 'n':
					Expect("null");
					return null;

				case '"':
					return ReadStringLiteral();

				default:
					if ('0' <= c && c <= '9' || c == '-')
					{
						return ReadNumericLiteral();
					}
					throw JsonError(string.Format(SR.ArgumentException_UnexpectedCharacter, (char)c));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int PeekChar()
		{
			if (!_has_peek)
			{
				_peek = ReadCharInternal();
				_has_peek = true;
			}
			return _peek;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int ReadCharInternal() => _cur == _len ? -1 : _r[_cur++];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int ReadChar()
		{
			int v = _has_peek ? _peek : ReadCharInternal();
			_has_peek = false;
#if DEBUG
			if (_prev_lf)
			{
				_line++;
				_column = 0;
				_prev_lf = false;
			}

			if (v == '\n')
			{
				_prev_lf = true;
			}
			_column++;
#endif
			return v;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SkipSpaces()
		{
			while (true)
			{
				switch (PeekChar())
				{
					case ' ':
					case '\t':
					case '\r':
					case '\n':
						ReadChar();
						continue;
					default:
						return;
				}
			}
		}

		// It could return either int, long, ulong, decimal or double, depending on the parsed value.
		private JsonValue ReadNumericLiteral()
		{
			var sb = _sb;
			sb.Clear();

			if (PeekChar() == '-')
			{
				sb.Append((char)ReadChar());
			}

			int c;
			int x = 0;
			bool zeroStart = PeekChar() == '0';
			for (; ; x++)
			{
				c = PeekChar();
				if (c < '0' || '9' < c)
				{
					break;
				}

				sb.Append((char)ReadChar());
				if (zeroStart && x == 1)
				{
					throw JsonError(SR.ArgumentException_LeadingZeros);
				}
			}

			if (x == 0) // Reached e.g. for "- "
			{
				throw JsonError(SR.ArgumentException_NoDigitFound);
			}

			// fraction
			bool hasFrac = false;
			int fdigits = 0;
			if (PeekChar() == '.')
			{
				hasFrac = true;
				sb.Append((char)ReadChar());
				if (PeekChar() < 0)
				{
					throw JsonError(SR.ArgumentException_ExtraDot);
				}

				while (true)
				{
					c = PeekChar();
					if (c < '0' || '9' < c)
					{
						break;
					}

					sb.Append((char)ReadChar());
					fdigits++;
				}
				if (fdigits == 0)
				{
					throw JsonError(SR.ArgumentException_ExtraDot);
				}
			}

			c = PeekChar();
			if (c != 'e' && c != 'E')
			{
				if (!hasFrac)
				{
					string value = sb.ToString();
					if (int.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out int valueInt))
					{
						return valueInt;
					}

					if (long.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out long valueLong))
					{
						return valueLong;
					}

					if (ulong.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out ulong valueUlong))
					{
						return valueUlong;
					}
				}

				if (decimal.TryParse(sb.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out decimal valueDecimal) && valueDecimal != 0)
				{
					return valueDecimal;
				}
			}
			else
			{
				// exponent
				sb.Append((char)ReadChar());
				if (PeekChar() < 0)
				{
					throw JsonError(SR.ArgumentException_IncompleteExponent);
				}

				c = PeekChar();
				if (c == '-')
				{
					sb.Append((char)ReadChar());
				}
				else if (c == '+')
				{
					sb.Append((char)ReadChar());
				}

				if (PeekChar() < 0)
				{
					throw JsonError(SR.ArgumentException_IncompleteExponent);
				}

				while (true)
				{
					c = PeekChar();
					if (c < '0' || '9' < c)
					{
						break;
					}

					sb.Append((char)ReadChar());
				}
			}

			return double.Parse(sb.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture);
		}

		private string ReadStringLiteral()
		{
			if (PeekChar() != '"')
			{
				throw JsonError(SR.ArgumentException_InvalidLiteralFormat);
			}

			ReadChar();
			StringBuilder sb = _sb;
			sb.Length = 0;
			while (true)
			{
				int c = ReadChar();
				if (c < 0)
				{
					throw JsonError(SR.ArgumentException_StringNotClosed);
				}

				if (c == '"')
				{
					return sb.ToString();
				}
				else if (c != '\\')
				{
					sb.Append((char)c);
					continue;
				}

				// escaped expression
				c = ReadChar();
				if (c < 0)
				{
					throw JsonError(SR.ArgumentException_IncompleteEscapeSequence);
				}
				switch (c)
				{
					case '"':
					case '\\':
					case '/':
						sb.Append((char)c);
						break;
					case 'b':
						sb.Append('\x8');
						break;
					case 'f':
						sb.Append('\f');
						break;
					case 'n':
						sb.Append('\n');
						break;
					case 'r':
						sb.Append('\r');
						break;
					case 't':
						sb.Append('\t');
						break;
					case 'u':
						ushort cp = 0;
						for (int i = 0; i < 4; i++)
						{
							cp <<= 4;
							if ((c = ReadChar()) < 0)
							{
								throw JsonError(SR.ArgumentException_IncompleteEscapeLiteral);
							}

							if ('0' <= c && c <= '9')
							{
								cp += (ushort)(c - '0');
							}
							if ('A' <= c && c <= 'F')
							{
								cp += (ushort)(c - 'A' + 10);
							}
							if ('a' <= c && c <= 'f')
							{
								cp += (ushort)(c - 'a' + 10);
							}
						}
						sb.Append((char)cp);
						break;
					default:
						throw JsonError(SR.ArgumentException_UnexpectedEscapeCharacter);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Expect(char expected)
		{
			int c;
			if ((c = ReadChar()) != expected)
			{
				throw JsonError(string.Format(SR.ArgumentException_ExpectedXButGotY, expected, (char)c));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Expect(string expected)
		{
			for (int i = 0; i < expected.Length; i++)
			{
				if (ReadChar() != expected[i])
				{
					throw JsonError(string.Format(SR.ArgumentException_ExpectedXDiferedAtY, expected, i));
				}
			}
		}

		private Exception JsonError(string msg)
		{
#if DEBUG
			return new ArgumentException(string.Format(SR.ArgumentException_MessageAt, msg, _line, _column));
#else
			return new ArgumentException(string.Format(SR.ArgumentException_MessageAt, msg, 1, _cur));
#endif
		}
	}
}