using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace S031.MetaStack.Json
{
	public class JsonValue
	{
		private static readonly Dictionary<Type, JsonType> _typesCache = new Dictionary<Type, JsonType>()
		{
			{typeof(bool), JsonType.Boolean },
			{typeof(byte), JsonType.Integer },
			{typeof(char), JsonType.String },
			{typeof(sbyte), JsonType.Integer },
			{typeof(short), JsonType.Integer },
			{typeof(ushort), JsonType.Integer },
			{typeof(int), JsonType.Integer },
			{typeof(uint), JsonType.Integer },
			{typeof(long), JsonType.Integer },
			{typeof(ulong), JsonType.Integer },
			{typeof(float), JsonType.Float },
			{typeof(double), JsonType.Float },
			{typeof(decimal), JsonType.Float },
			{typeof(DateTime), JsonType.Date },
			{typeof(string), JsonType.String },
			{typeof(Uri), JsonType.Uri },
			{typeof(Guid), JsonType.Guid },
			{typeof(byte[]), JsonType.Bytes },
			{typeof(DateTimeOffset), JsonType.String },
			{typeof(TimeSpan), JsonType.String },
		};

		private readonly object _value;
		private readonly JsonType _type;

		public JsonValue()
		{
			_value = null;
			_type = JsonType.Null;
		}

		public JsonValue(object value)
		{
			if (value == null)
			{
				_type = JsonType.Null;
				_value = value;
			}
			else
			{
				Type t = value.GetType();
				if (!_typesCache.TryGetValue(t, out _type))
					//throw new NotSupportedException();
					_type = JsonType.Raw;
				_value = value;
			}
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

		public static implicit operator bool(JsonValue value) => (bool)value.Value;

		public static implicit operator byte(JsonValue value) => (byte)value.Value;

		public static implicit operator char(JsonValue value) => (char)value.Value;

		public static implicit operator decimal(JsonValue value) => (decimal)value.Value;

		public static implicit operator double(JsonValue value) => (double)value.Value;

		public static implicit operator float(JsonValue value) => (float)value.Value;

		public static implicit operator int(JsonValue value) => (int)value.Value;

		public static implicit operator long(JsonValue value) => (long)value.Value;

		public static implicit operator sbyte(JsonValue value) => (sbyte)value.Value;

		public static implicit operator short(JsonValue value) => (short)value.Value;

		public static implicit operator string(JsonValue value) => value != null ?
				(string)value.Value :
				null;

		public static implicit operator uint(JsonValue value) => (uint)value.Value;

		public static implicit operator ulong(JsonValue value) => (ulong)value.Value;

		public static implicit operator ushort(JsonValue value) => (ushort)value.Value;

		public static implicit operator DateTime(JsonValue value) => (DateTime)value.Value;

		public static implicit operator DateTimeOffset(JsonValue value) => (DateTimeOffset)value.Value;

		public static implicit operator TimeSpan(JsonValue value) => (TimeSpan)value.Value;

		public static implicit operator Guid(JsonValue value) => (Guid)value.Value;

		public static implicit operator Uri(JsonValue value) => (Uri)value.Value;

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

		public virtual bool IsEmpty()
			=> _type == JsonType.Null;

		public virtual object GetValue()
			=> _value;
		public override string ToString()
			=> new JsonWriter(Formatting.None)
			.WriteValue(this)
			.ToString();
	}
}