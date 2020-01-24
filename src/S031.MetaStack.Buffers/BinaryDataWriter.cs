using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Buffers
{
	public enum ExportedDataTypes : byte
	{
		@null = 0,
		@false = 1,
		@true = 2,
		@sbyte = 3,
		@byte = 4,
		@short = 5,
		@ushort = 6,
		@int = 7,
		@uint = 8,
		@long = 9,
		@ulong = 10,
		@float = 11,
		@double = 12,
		@decimal = 13,
		@string = 14,
		@byteArray = 15,
		@guid = 16,
		@url = 17,
		@date = 18,
		@asciiString = 19,
		@utf8String = 20,
		@object = 253,
		@array = 254,
		@none = 255
	}
	public ref struct BinaryDataWriter
	{
		private const int base_size = 4;
		private static readonly int[] _sizerStatistic = new int[32];

		byte[] _buffer;
		int _index;

		public BinaryDataWriter(int capacity)
		{
			capacity = GetCapacity(capacity);
			_buffer = new byte[capacity];
			_index = 0;
		}

		private static int GetCapacity(int value)
		{
			int maxValue = _sizerStatistic[0];
			int indexForMaxValue = 0;
			for (int i = 1; i < _sizerStatistic.Length; i++) 
			{ 
				if (_sizerStatistic[i] > maxValue)
				{
					maxValue = _sizerStatistic[i];
					indexForMaxValue = i;
				}
			}
			return Math.Max(Convert.ToInt32(Math.Pow(2, base_size + indexForMaxValue)), value);
		}

		public int Length
			=> _index;

		public unsafe byte[] GetBytes()
		{
			//Update statistics
			int newPosition = Math.Min(Convert.ToInt32(Math.Log(_index, 2)) - base_size, _sizerStatistic.Length);
			_sizerStatistic[newPosition]++;

			byte[] result = new byte[_index];

			fixed (byte* source = _buffer)
			fixed (byte* dest = result)
				Buffer.MemoryCopy(source, dest, _index, _index);
			return result;

		}

		public void WriteNull()
		{
			CheckAndResizeBuffer(1);
			_buffer[_index] = (byte)ExportedDataTypes.@null;
			_index++;
		}

		public void Write(bool value)
		{
			CheckAndResizeBuffer(sizeof(bool));
			_buffer[_index] = (byte)(value ? ExportedDataTypes.@true : ExportedDataTypes.@false);
			_index++;
		}

		public unsafe void Write(sbyte value)
		{
			CheckAndResizeBuffer(sizeof(sbyte) + 1);
			_buffer[_index] = (byte)ExportedDataTypes.@sbyte;
			_index++;

			fixed (byte* ptr = _buffer)
				*(sbyte*)(ptr + _index) = value;
			_index++;
		}

		public void Write(byte value)
		{
			CheckAndResizeBuffer(sizeof(byte) + 1);
			_buffer[_index] = (byte)ExportedDataTypes.@byte;
			_index++;

			_buffer[_index] = value;
			_index++;
		}

		public unsafe void Write(short value, ExportedDataTypes type = ExportedDataTypes.@short)
		{
			CheckAndResizeBuffer(sizeof(short) + 1);
			_buffer[_index] = (byte)type;
			_index++;

			fixed (byte* ptr = _buffer)
				*(short*)(ptr + _index) = value;
			_index += sizeof(short);
		}
		public void Write(ushort value)
			=> Write((short)value, ExportedDataTypes.@ushort);

		public unsafe void Write(int value, ExportedDataTypes type = ExportedDataTypes.@int)
		{
			CheckAndResizeBuffer(sizeof(int) + 1);
			_buffer[_index] = (byte)type;
			_index++;

			fixed (byte* ptr = _buffer)
				*(int*)(ptr + _index) = value;
			_index += sizeof(int);
		}

		public unsafe void Write(uint value)
			=> Write((int)value, ExportedDataTypes.@uint);

		public unsafe void Write(long value, ExportedDataTypes type = ExportedDataTypes.@long)
		{
			CheckAndResizeBuffer(sizeof(long) + 1);
			_buffer[_index] = (byte)type;
			_index++;

			fixed (byte* ptr = _buffer)
				*(long*)(ptr + _index) = value;
			_index += sizeof(long);
		}

		public unsafe void Write(decimal value)
		{
			CheckAndResizeBuffer(sizeof(decimal) + 1);
			_buffer[_index] = (byte)ExportedDataTypes.@decimal;
			_index++;

			fixed (byte* ptr = _buffer)
				*(decimal*)(ptr + _index) = value;
			_index += sizeof(decimal);
		}

		public void Write(ulong value)
			=> Write((long)value, ExportedDataTypes.@ulong);

		public unsafe void Write(float value)
			=> Write(*(int*)&value, ExportedDataTypes.@float);

		public unsafe void Write(double value)
			=> Write(*(long*)&value, ExportedDataTypes.@double);

		public void Write(DateTime value)
			=> Write(value.Ticks, ExportedDataTypes.@date);
		
		public void Write(TimeSpan value)
			=> Write(value.Ticks, ExportedDataTypes.@date);

		public unsafe void Write(char[] value)
		{
			var encoding = Encoding.UTF8;

			byte[] data = encoding.GetBytes(value);
			int size = data.Length;

			CheckAndResizeBuffer(size + sizeof(int) + 1);
			_buffer[_index] = (byte)ExportedDataTypes.utf8String;
			_index++;

			fixed (byte* ptr = _buffer)
				*(int*)(ptr + _index) = size;
			_index += sizeof(int);

			fixed (byte* source = data)
			fixed (byte* dest = _buffer)
				Buffer.MemoryCopy(source, dest + _index, size, size);
			_index += size;
		}

		//static byte[] EncodeToBytes(string str)
		//{
		//	byte[] bytes = new byte[str.Length * sizeof(char)];
		//	System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
		//	return bytes;
		//}
		//static string DecodeToString(byte[] bytes)
		//{
		//	char[] chars = new char[bytes.Length / sizeof(char)];
		//	System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
		//	return new string(chars);
		//}

		/// <summary>
		/// Write string in two byte array
		/// </summary>
		/// <param name="value"></param>
		public unsafe void Write(string value)
		{
			int size = value.Length * sizeof(char);
			CheckAndResizeBuffer(size + sizeof(int) + 1);

			_buffer[_index] = (byte)ExportedDataTypes.@string;
			_index++;

			fixed (byte* ptr = _buffer)
				*(int*)(ptr + _index) = size;
			_index += sizeof(int);

			if (size > 0)
			{
				fixed (char* source = value)
				fixed (byte* dest = &_buffer[_index])
					Buffer.MemoryCopy(source, dest, size, size);
				_index += size;
			}
		}

		/// <summary>
		/// Write string to one byte array (for ascii string symbols only)
		/// </summary>
		/// <param name="value"></param>
		/// <param name="size"></param>
		public unsafe void Write(char* value, int size)
		{
			CheckAndResizeBuffer(size + sizeof(int) + 1);

			_buffer[_index] = (byte)ExportedDataTypes.asciiString;
			_index++;

			fixed (byte* ptr = _buffer)
				*(int*)(ptr + _index) = size;
			_index += sizeof(int);

			if (size > 0)
			{
				fixed (byte* dest = &_buffer[_index])
				{
					for (int i = 0; i < size; i++)
						dest[i] = (byte)value[i];
				}
				_index += size;
			}
		}

		public unsafe void Write(byte[] value)
		{
			int size = value.Length;// * sizeof(char);
			CheckAndResizeBuffer(size + sizeof(int) + 1);

			_buffer[_index] = (byte)ExportedDataTypes.byteArray;
			_index++;

			fixed (byte* ptr = _buffer)
				*(int*)(ptr + _index) = size;
			_index += sizeof(int);

			if (size > 0)
			{
				fixed (byte* source = value)
				fixed (byte* dest = _buffer)
					Buffer.MemoryCopy(source, dest + _index, size, size);
				_index += size;
			}
		}
		public unsafe void Write(Guid value)
		{
			int size = 36;
			CheckAndResizeBuffer(size + 1);

			_buffer[_index] = (byte)ExportedDataTypes.guid;
			_index++;

			fixed (byte* source = value.ToByteArray())
			fixed (byte* dest = _buffer)
				Buffer.MemoryCopy(source, dest + _index, size, size);
			_index += size;
		}

		public unsafe void WriteMapHeader(int count)
			=> Write(count, ExportedDataTypes.@object);

		public unsafe void WriteArrayHeader(int count)
			=> Write(count, ExportedDataTypes.@array);

		private unsafe void CheckAndResizeBuffer(int sizeHint)
		{
			int availableSpace = _buffer.Length - _index;

			if (sizeHint > availableSpace)
			{
				int growBy = Math.Max(sizeHint, _buffer.Length);

				int newSize = checked(_buffer.Length + growBy);

				byte[] oldBuffer = _buffer;
				_buffer = new byte[newSize];

				fixed (byte* source = oldBuffer)
				fixed (byte* dest = _buffer)
					Buffer.MemoryCopy(source, dest, newSize, _index);
			}
		}
	}
}
