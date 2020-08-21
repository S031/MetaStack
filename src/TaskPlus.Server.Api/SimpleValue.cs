using Microsoft.VisualBasic.CompilerServices;
using S031.MetaStack.Buffers;
using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DXApplication1
{
	readonly struct SimpleValue2
	{
		private readonly ValueType _value;
		private readonly byte[] _buffer;
		private readonly ExportedDataTypes _type;

		public SimpleValue2(int value)
		{
			_value = value;
			_buffer = null;
			_type = ExportedDataTypes.@int;
		}

		public SimpleValue2(string value)
		{
			_value = false;
			_buffer = new BinaryDataWriter(value.Length * sizeof(char) + sizeof(int) + 1)
				.Write(value)
				.GetBytes();
			_type = ExportedDataTypes.@string;
		}
		public static implicit operator string(SimpleValue2 value)
			=> value._type == ExportedDataTypes.@string 
			? new BinaryDataReader((BinaryDataBuffer)value._buffer).ReadString()
			: throw new InvalidCastException();

	}

	//public readonly struct SimpleValue
	//{
	//	private readonly ValueType _value;
	//	private readonly byte[] _buffer;
	//	private readonly ExportedDataTypes _type;

	//	public SimpleValue(int value)
	//	{
	//		_value = value;
	//		_buffer = null;
	//		_type = ExportedDataTypes.@int;
	//	}

	//	public SimpleValue(string value)
	//	{
	//		_value = false;
	//		_buffer = new BinaryDataWriter(value.Length * sizeof(char) + sizeof(int) + 1)
	//			.Write(value)
	//			.GetBytes();
	//		_type = ExportedDataTypes.@string;
	//	}

	//	public SimpleValue(DateTime value)
	//	{
	//		_value = value.Ticks;
	//		_string = null;
	//		_type = SimpleValueType.DateTime;
	//	}

	//	public override string ToString()
	//	{
	//		switch (_type)
	//		{
	//			case SimpleValueType.String:
	//				return _string;
	//			case SimpleValueType.DateTime:
	//				return new DateTime((long)_value).ToString();
	//			default:
	//				return _value.ToString();
	//		}				
	//	}


	//	public override int GetHashCode()
	//		=> _type == SimpleValueType.String ? _string.GetHashCode() : _value.GetHashCode();

	//	public override bool Equals(object obj)
	//	{
	//		if (_type == SimpleValueType.String && obj is string)
	//			return _string.Equals((string)obj, StringComparison.Ordinal);
	//		else if (_type == SimpleValueType.DateTime && obj is DateTime)
	//			return (long)_value == ((DateTime)obj).Ticks;
	//		else if (_type == SimpleValueType.Number && obj is ValueType)
	//			return _value.Equals(obj);
	//		throw new InvalidCastException();
	//	}

	//	public static implicit operator string(SimpleValue value)
	//		=> value._type == SimpleValueType.String ? value._string : throw new InvalidCastException();


	//	public static implicit operator byte(SimpleValue value)
	//		=> !value._isStringValue ? (byte)value._value : throw new InvalidCastException();

	//	public static implicit operator uint(SimpleValue value)
	//		=> !value._isStringValue ? (uint)value._value : throw new InvalidCastException();

	//	public static implicit operator int(SimpleValue value)
	//		=> !value._isStringValue ? (int)value._value : throw new InvalidCastException();

	//	public static implicit operator long(SimpleValue value)
	//		=> !value._isStringValue ? (long)value._value : throw new InvalidCastException();

	//	public static implicit operator float(SimpleValue value)
	//		=> !value._isStringValue ? (float)value._value : throw new InvalidCastException();

	//	public static implicit operator double(SimpleValue value)
	//		=> !value._isStringValue ? (double)value._value : throw new InvalidCastException();

	//	public static implicit operator decimal(SimpleValue value)
	//		=> !value._isStringValue ? (decimal)value._value : throw new InvalidCastException();

	//	public static explicit operator SimpleValue(string value)
	//		=> new SimpleValue(value);

	//	public static explicit operator SimpleValue(int value)
	//		=> new SimpleValue(value);

	//	//public static SimpleValue operator =(SimpleValue s, int value) { new SimpleValue(value)}

	//}

	//public static class Defaults
	//{
	//	public static readonly SimpleValue WebMethod = (SimpleValue)"POST";
	//	public static readonly SimpleValue Method = (SimpleValue)0;

	//	//public static ObjectDefault WebMethod
	//	//	=> new ObjectDefault(false);
	//}
}
