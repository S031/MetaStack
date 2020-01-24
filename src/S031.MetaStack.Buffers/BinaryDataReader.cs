using System;
using System.Text;

namespace S031.MetaStack.Buffers
{
	public ref struct BinaryDataReader
	{
		private readonly byte[] _buffer;
		private int _index;
		private int _count;

		public BinaryDataReader(byte[] source)
		{
			_buffer = source;
			_index = 0;
			_count = _buffer.Length;
		}

		public int Lenght
			=> _count;

		public int Position
		{
			get => _index;
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
		
		public Byte ReadByte()
		{
			byte result = _buffer[_index];
			_index++;
			return result;
		}

		public unsafe byte[] Readbytes()
		{
			if (_index >= _count)
				throw new IndexOutOfRangeException();

			int size = ReadInt32();
			if (_index + size > _count)
				throw new IndexOutOfRangeException();

			if (size > 0)
			{
				byte[] data;
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

			byte[] data;
			fixed (byte* source = &_buffer[_index])
			fixed (byte* dest =  data)
				Buffer.MemoryCopy(source, dest, guid_len, guid_len);

			Guid result = new Guid(data);
			_index += guid_len;
			return result;
		}
	}
}
