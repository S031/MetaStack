// Licensed to the .NET Foundation under one or more agreements.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;
using System.IO;
using System.Text;
using SR = S031.MetaStack.Json.Resources.Strings;

namespace S031.MetaStack.Json
{
	public partial class JsonValue
	{
		private static readonly byte[] s_trueBytes = Encoding.UTF8.GetBytes("true");
		private static readonly byte[] s_falseBytes = Encoding.UTF8.GetBytes("false");
		private readonly object _value;
		private readonly JsonType _type;

		public JsonValue()
		{
			_value = null;
			_type = JsonType.Null;
		}

		public JsonValue(bool value)
		{
			_value = value;
			_type = JsonType.Boolean;
		}

		public JsonValue(byte value)
		{
			_value = value;
			_type = JsonType.Integer;
		}

		public JsonValue(char value)
		{
			_value = value;
			_type = JsonType.String;
		}

		public JsonValue(decimal value)
		{
			_value = value;
			_type = JsonType.Float;
		}

		public JsonValue(double value)
		{
			_value = value;
			_type = JsonType.Float;
		}

		public JsonValue(float value)
		{
			_value = value;
			_type = JsonType.Float;
		}

		public JsonValue(int value)
		{
			_value = value;
			_type = JsonType.Integer;
		}

		public JsonValue(long value)
		{
			_value = value;
			_type = JsonType.Integer;
		}

		public JsonValue(sbyte value)
		{
			_value = value;
			_type = JsonType.Integer;
		}

		public JsonValue(short value)
		{
			_value = value;
			_type = JsonType.Integer;
		}

		public JsonValue(string value)
		{
			_value = value;
			_type = JsonType.String;
		}

		public JsonValue(DateTime value)
		{
			_value = value;
			_type = JsonType.Date;
		}

		public JsonValue(uint value)
		{
			_value = value;
			_type = JsonType.Integer;
		}

		public JsonValue(ulong value)
		{
			_value = value;
			_type = JsonType.Integer;
		}

		public JsonValue(ushort value)
		{
			_value = value;
			_type = JsonType.Integer;
		}

		public JsonValue(DateTimeOffset value)
		{
			_value = value;
			_type = JsonType.String;
		}

		public JsonValue(Guid value)
		{
			_value = value;
			_type = JsonType.Guid;
		}

		public JsonValue(TimeSpan value)
		{
			_value = value;
			_type = JsonType.String;
		}

		public JsonValue(Uri value)
		{
			_value = value;
			_type = JsonType.String;
		}

		public JsonValue(byte[] value)
		{
			_value = value;
			_type = JsonType.Bytes;
		}

		internal object Value => _value;

		// CLI -> JsonValue

		public static implicit operator JsonValue(bool value) => new JsonValue(value);

		public static implicit operator JsonValue(byte value) => new JsonValue(value);

		public static implicit operator JsonValue(char value) => new JsonValue(value);

		public static implicit operator JsonValue(decimal value) => new JsonValue(value);

		public static implicit operator JsonValue(double value) => new JsonValue(value);

		public static implicit operator JsonValue(float value) => new JsonValue(value);

		public static implicit operator JsonValue(int value) => new JsonValue(value);

		public static implicit operator JsonValue(long value) => new JsonValue(value);

		public static implicit operator JsonValue(sbyte value) => new JsonValue(value);

		public static implicit operator JsonValue(short value) => new JsonValue(value);

		public static implicit operator JsonValue(string value) => new JsonValue(value);

		public static implicit operator JsonValue(uint value) => new JsonValue(value);

		public static implicit operator JsonValue(ulong value) => new JsonValue(value);

		public static implicit operator JsonValue(ushort value) => new JsonValue(value);

		public static implicit operator JsonValue(DateTime value) => new JsonValue(value);

		public static implicit operator JsonValue(DateTimeOffset value) => new JsonValue(value);

		public static implicit operator JsonValue(Guid value) => new JsonValue(value);

		public static implicit operator JsonValue(TimeSpan value) => new JsonValue(value);

		public static implicit operator JsonValue(Uri value) => new JsonValue(value);

		// JsonValue -> CLI

		public static implicit operator bool(JsonValue value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return Convert.ToBoolean(((JsonValue)value).Value, CultureInfo.InvariantCulture);
		}

		public static implicit operator byte(JsonValue value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return Convert.ToByte(((JsonValue)value).Value, CultureInfo.InvariantCulture);
		}

		public static implicit operator char(JsonValue value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return Convert.ToChar(((JsonValue)value).Value, CultureInfo.InvariantCulture);
		}

		public static implicit operator decimal(JsonValue value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return Convert.ToDecimal(((JsonValue)value).Value, CultureInfo.InvariantCulture);
		}

		public static implicit operator double(JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			return Convert.ToDouble(((JsonValue)value).Value, CultureInfo.InvariantCulture);
		}

		public static implicit operator float(JsonValue value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return Convert.ToSingle(((JsonValue)value).Value, CultureInfo.InvariantCulture);
		}

		public static implicit operator int(JsonValue value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return Convert.ToInt32(((JsonValue)value).Value, CultureInfo.InvariantCulture);
		}

		public static implicit operator long(JsonValue value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return Convert.ToInt64(((JsonValue)value).Value, CultureInfo.InvariantCulture);
		}

		public static implicit operator sbyte(JsonValue value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return Convert.ToSByte(((JsonValue)value).Value, CultureInfo.InvariantCulture);
		}

		public static implicit operator short(JsonValue value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return Convert.ToInt16(((JsonValue)value).Value, CultureInfo.InvariantCulture);
		}

		public static implicit operator string(JsonValue value)
		{
			return value != null ?
				(string)((JsonValue)value).Value :
				null;
		}

		public static implicit operator uint(JsonValue value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return Convert.ToUInt32(((JsonValue)value).Value, CultureInfo.InvariantCulture);
		}

		public static implicit operator ulong(JsonValue value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return Convert.ToUInt64(((JsonValue)value).Value, CultureInfo.InvariantCulture);
		}

		public static implicit operator ushort(JsonValue value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return Convert.ToUInt16(((JsonValue)value).Value, CultureInfo.InvariantCulture);
		}

		public static implicit operator DateTime(JsonValue value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return (DateTime)((JsonValue)value).Value;
		}

		public static implicit operator DateTimeOffset(JsonValue value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return (DateTimeOffset)((JsonValue)value).Value;
		}

		public static implicit operator TimeSpan(JsonValue value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return (TimeSpan)((JsonValue)value).Value;
		}

		public static implicit operator Guid(JsonValue value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return (Guid)((JsonValue)value).Value;
		}

		public static implicit operator Uri(JsonValue value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			return (Uri)((JsonValue)value).Value;
		}

		public virtual JsonType JsonType
			=> _type;

		private static readonly NumberFormatInfo _nfi = new CultureInfo("en-US").NumberFormat;
		internal string GetFormattedString()
		{
			if (Value == null)
				return "null";
			switch (JsonType)
			{
				case JsonType.Integer:
					return Convert.ToInt64(_value).ToString("g", _nfi);
				case JsonType.Float:
					return Convert.ToDouble(_value).ToString("g", _nfi);
				case JsonType.Date:
					return ((DateTime)_value).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
				case JsonType.String:
					return (string)_value;
				case JsonType.Bytes:
					return Convert.ToBase64String((byte[])_value);
				case JsonType.Array:
				case JsonType.Object:
					throw new NotImplementedException();
				default:
					return _value.ToString();
			}
		}
	}
}