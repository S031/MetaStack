using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace S031.MetaStack.Buffers
{
	public class BinaryDataReader
	{
		private readonly byte[] _buffer;
		private int _index;
		private readonly int _count;

		public BinaryDataReader(byte[] source)
		{
			_buffer = source;
			_index = 0;
			_count = _buffer.Length;
		}

		public byte[] Source
			=> _buffer;

		public int Lenght
			=> _count;

		public int Position
		{
			get { return _index; }
			set
			{
				if (value < 0 || value > _count)
					throw new ArgumentOutOfRangeException(nameof(value));
				_index = value;
			}
		}			

		public ExportedDataTypes ReadNext()
		{
			if (_index >= _count)
				return ExportedDataTypes.none;
			
			var result = (ExportedDataTypes)_buffer[_index];
			_index++;
			return result;
		}

		public unsafe string ReadString()
		{
			if (_index >= _count)
				throw new IndexOutOfRangeException();

			int size = ReadInt32();
			if (_index + size > _count)
				throw new IndexOutOfRangeException();

			if (size > 0)
			{
				fixed (byte* source = &_buffer[_index])
				fixed (char* dest = new char[size])
				{
					Buffer.MemoryCopy(source, dest, size, size);
					_index += size;
					return new string(dest);
				};
			}
			return string.Empty;
		}

		public unsafe string ReadAsciiString()
		{
			if (_index >= _count)
				throw new IndexOutOfRangeException();

			int size = ReadInt32();
			if (_index + size > _count)
				throw new IndexOutOfRangeException();

			if (size > 0)
			{
				fixed (byte* source = &_buffer[_index])
				fixed (char* dest = new char[size])
				{
					for (int i = 0; i < size; i++)
						dest[i] = (char)source[i];
					_index += size;
					return new string(dest);
				};
			}
			return string.Empty;
		}

		public unsafe string ReadUtf8String()
		{
			if (_index >= _count)
				throw new IndexOutOfRangeException();

			int size = ReadInt32();
			if (_index + size > _count)
				throw new IndexOutOfRangeException();
			if (size > 0)
			{
				fixed (byte* source = &_buffer[_index])
				{
					_index += size;
					return Encoding.UTF8.GetString(source, size);
				};
			}
			return string.Empty;
		}

		public bool ReadBool()
			=> ReadByte() == 0 ? false : true;

		public Byte ReadByte()
		{
			byte result = _buffer[_index];
			_index++;
			return result;
		}

		public unsafe byte[] ReadBytes()
		{
			if (_index >= _count)
				throw new IndexOutOfRangeException();

			int size = ReadInt32();
			if (_index + size > _count)
				throw new IndexOutOfRangeException();

			if (size > 0)
			{
				byte[] data = new byte[size];
				fixed (byte* source = &_buffer[_index])
				fixed (byte* dest = data)
					Buffer.MemoryCopy(source, dest, size, size);
				_index += size;
				return data;
			}
			return new byte[] { };
		}

		public unsafe sbyte ReadSByte()
		{
			sbyte result;
			fixed (byte* p = &_buffer[_index])
				result = *(sbyte*)p;
			_index += sizeof(sbyte);
			return result;
		}

		public short ReadInt16()
		{
			short result = BitConverter.ToInt16(_buffer, _index);
			_index+=sizeof(short);
			return result;
		}
		
		public ushort ReadUInt16()
		{
			ushort result = BitConverter.ToUInt16(_buffer, _index);
			_index+=sizeof(UInt16);
			return result;
		}
		
		public int ReadInt32()
		{
			int result = BitConverter.ToInt32(_buffer, _index);
			_index+=sizeof(int);
			return result;
		}
		
		public uint ReadUInt32()
		{
			uint result = BitConverter.ToUInt32(_buffer, _index);
			_index+=sizeof(UInt32);
			return result;
		}

		public long ReadInt64()
		{
			long result = BitConverter.ToInt64(_buffer, _index);
			_index+=sizeof(long);
			return result;
		}

		public ulong ReadUInt64()
		{
			ulong result = BitConverter.ToUInt64(_buffer, _index);
			_index+=sizeof(ulong);
			return result;
		}

		public float ReadSingle()
		{
			float result = BitConverter.ToSingle(_buffer, _index);
			_index+=sizeof(float);
			return result;
		}

		public double ReadDouble()
		{
			double result = BitConverter.ToDouble(_buffer, _index);
			_index+=sizeof(double);
			return result;
		}

		public unsafe decimal ReadDecimal()
		{
			decimal result;
			fixed (byte* p = &_buffer[_index])
			{
				result = *(decimal*)p;
			}
			_index += sizeof(decimal);
			return result;
		}

		public DateTime ReadDate()
			=> new DateTime(ReadInt64());
		
		public unsafe Guid ReadGuid()
		{
			const int guid_len = 36;

			byte[] data = new byte[guid_len];
			fixed (byte* source = &_buffer[_index])
			fixed (byte* dest =  data)
				Buffer.MemoryCopy(source, dest, guid_len, guid_len);

			Guid result = new Guid(data);
			_index += guid_len;
			return result;
		}

		public void ReadRaw(IDictionary<string, object> map)
		{
			int count = ReadInt32();
			for (int i = 0; i < count; i++)
			{
				var t = ReadNext();
				if (t != ExportedDataTypes.asciiString)
					throw new FormatException("Expected property name string");

				string propertyName = ReadAsciiString();
				if (string.IsNullOrEmpty(propertyName))
					throw new FormatException("Property name string must have a value");

				map[propertyName] = ReadValue();
			}
		}

		private void ReadArrayRaw(IList<object> array)
		{
			int count = ReadInt32();
			for (int i = 0; i < count; i++)
				array.Add(ReadValue());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public object ReadValue()
		{
			var t = ReadNext();
			switch (t)
			{
				case ExportedDataTypes.@int:
					return ReadInt32();
				case ExportedDataTypes.asciiString:
					return ReadAsciiString();
				case ExportedDataTypes.@string:
					return ReadString();
				case ExportedDataTypes.utf8String:
					return ReadUtf8String();
				case ExportedDataTypes.@bool:
					return ReadByte() == 1 ? true : false;
				case ExportedDataTypes.@byte:
					return ReadByte();
				case ExportedDataTypes.@short:
					return ReadInt16();
				case ExportedDataTypes.@ushort:
					return ReadUInt16();
				case ExportedDataTypes.@uint:
					return ReadUInt32();
				case ExportedDataTypes.@long:
					return ReadInt64();
				case ExportedDataTypes.@ulong:
					return ReadUInt64();
				case ExportedDataTypes.@float:
					return ReadSingle();
				case ExportedDataTypes.@double:
					return ReadDouble();
				case ExportedDataTypes.@decimal:
					return ReadDecimal();
				case ExportedDataTypes.dateTime:
					return ReadDate();
				case ExportedDataTypes.@guid:
					return ReadGuid();
				case ExportedDataTypes.@null:
					return DBNull.Value;
				case ExportedDataTypes.none:
					return null;
				case ExportedDataTypes.byteArray:
					return ReadBytes();
				case ExportedDataTypes.@object:
					MapTable<string, object> map = new MapTable<string, object>();
					ReadRaw(map);
					return map;
				case ExportedDataTypes.@array:
					List<object> a = new List<object>();
					ReadArrayRaw(a);
					return a;
				default:
					throw new FormatException($"Not supported ExportedDataType {t}");
			}
		}
	}
}
