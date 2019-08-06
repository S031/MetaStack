using System;
using System.Globalization;
using System.Text;

namespace S031.MetaStack.Json
{
	public class JsonValue
	{
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

		public static implicit operator JsonValue(bool value)
		{
			return new JsonValue(value);
		}

		public static implicit operator JsonValue(byte value)
		{
			return new JsonValue(value);
		}

		public static implicit operator JsonValue(char value)
		{
			return new JsonValue(value);
		}

		public static implicit operator JsonValue(decimal value)
		{
			return new JsonValue(value);
		}

		public static implicit operator JsonValue(double value)
		{
			return new JsonValue(value);
		}

		public static implicit operator JsonValue(float value)
		{
			return new JsonValue(value);
		}

		public static implicit operator JsonValue(int value)
		{
			return new JsonValue(value);
		}

		public static implicit operator JsonValue(long value)
		{
			return new JsonValue(value);
		}

		public static implicit operator JsonValue(sbyte value)
		{
			return new JsonValue(value);
		}

		public static implicit operator JsonValue(short value)
		{
			return new JsonValue(value);
		}

		public static implicit operator JsonValue(string value)
		{
			return new JsonValue(value);
		}

		public static implicit operator JsonValue(uint value)
		{
			return new JsonValue(value);
		}

		public static implicit operator JsonValue(ulong value)
		{
			return new JsonValue(value);
		}

		public static implicit operator JsonValue(ushort value)
		{
			return new JsonValue(value);
		}

		public static implicit operator JsonValue(DateTime value)
		{
			return new JsonValue(value);
		}

		public static implicit operator JsonValue(DateTimeOffset value)
		{
			return new JsonValue(value);
		}

		public static implicit operator JsonValue(Guid value)
		{
			return new JsonValue(value);
		}

		public static implicit operator JsonValue(TimeSpan value)
		{
			return new JsonValue(value);
		}

		public static implicit operator JsonValue(Uri value)
		{
			return new JsonValue(value);
		}

		// JsonValue -> CLI

		public static implicit operator bool(JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return (bool)value.Value;
		}

		public static implicit operator byte(JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return (byte)value.Value;
		}

		public static implicit operator char(JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return (char)value.Value;
		}

		public static implicit operator decimal(JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return (decimal)value.Value;
		}

		public static implicit operator double(JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			return (double)value.Value;
		}

		public static implicit operator float(JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return (float)value.Value;
		}

		public static implicit operator int(JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return (int)value.Value;
		}

		public static implicit operator long(JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return (long)value.Value;
		}

		public static implicit operator sbyte(JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return (sbyte)value.Value;
		}

		public static implicit operator short(JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return (short)value.Value;
		}

		public static implicit operator string(JsonValue value)
		{
			return value != null ?
				(string)value.Value :
				null;
		}

		public static implicit operator uint(JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return (uint)value.Value;
		}

		public static implicit operator ulong(JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return (ulong)value.Value;
		}

		public static implicit operator ushort(JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return (ushort)value.Value;
		}

		public static implicit operator DateTime(JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return (DateTime)value.Value;
		}

		public static implicit operator DateTimeOffset(JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return (DateTimeOffset)value.Value;
		}

		public static implicit operator TimeSpan(JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return (TimeSpan)value.Value;
		}

		public static implicit operator Guid(JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return (Guid)value.Value;
		}

		public static implicit operator Uri(JsonValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			return (Uri)value.Value;
		}

		public virtual JsonType JsonType
			=> _type;

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
	}
}