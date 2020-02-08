using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Buffers
{
	/// <summary>
	/// Inherited from <see cref="MdbType"/> 
	/// </summary>
	public enum ExportedDataTypes : byte
	{
		@none = MdbType.none,
		@object = MdbType.@object,
		@null = MdbType.@null,
		@bool = MdbType.@bool,
		@char = MdbType.@char,
		@sbyte = MdbType.@sbyte,
		@byte = MdbType.@byte,
		@short = MdbType.@short,
		@ushort = MdbType.@ushort,
		@int = MdbType.@int,
		@uint = MdbType.@uint,
		@long = MdbType.@long,
		@ulong = MdbType.@ulong,
		@float = MdbType.@float,
		@double = MdbType.@double,
		@decimal = MdbType.@decimal,
		dateTime = MdbType.@dateTime,
		@guid = MdbType.@guid,
		@string = MdbType.@string,
		byteArray = MdbType.@byteArray,

		@asciiString = MdbType.@byteArray + 1,
		@utf8String = @asciiString + 1,
		@array = @utf8String + 1,
	}


	public struct BinaryDataWriter
	{
		unsafe static readonly Action<BinaryDataWriter, object>[] _delegates = new Action<BinaryDataWriter, object>[]
		{
			(writer, value)=>writer.Write(),
			(writer, value)=>
			{
				Type t = value.GetType();
				if (typeof(IDictionary<string, object>).IsAssignableFrom(t))
					writer.Write((IDictionary<string, object>)value);
				else if (typeof(IList<object>).IsAssignableFrom(t))
					writer.Write((IList<object>)value);
			},
			(writer, value)=>writer.WriteNull(),
			(writer, value)=>writer.Write((bool)value),
			(writer, value)=>writer.Write((char)value),
			(writer, value)=>writer.Write((sbyte)value),
			(writer, value)=>writer.Write((byte)value),
			(writer, value)=>writer.Write((short)value),
			(writer, value)=>writer.Write((ushort)value),
			(writer, value)=>writer.Write((int)value),
			(writer, value)=>writer.Write((uint)value),
			(writer, value)=>writer.Write((long)value),
			(writer, value)=>writer.Write((ulong)value),
			(writer, value)=>writer.Write((float)value),
			(writer, value)=>writer.Write((double)value),
			(writer, value)=>writer.Write((decimal)value),
			(writer, value)=>writer.Write((DateTime)value),
			(writer, value)=>writer.Write((Guid)value),
			(writer, value)=>writer.Write((string)value),
			(writer, value)=>writer.Write((byte[])value),
			(writer, value) =>
			{
				string source = (string)value;
				int len = source.Length;
				fixed (char * ptr = source)
					writer.Write(ptr, len);
			},
			(writer, value) => writer.Write((char[])value),
			(writer, value) => writer.Write((IList<object>)value)
		};


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

		internal BinaryDataWriter(byte[] source)
		{
			_buffer = source;
			_index = _buffer.Length;
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

		public int Position
		{
			get { return _index; }
			set
			{
				if (value < 0 || value > Source.Length)
					throw new ArgumentOutOfRangeException(nameof(value));
				_index = value;
			}
		}

		public int Length
			=> _index;

		public byte[] Source
			=> _buffer;

		public unsafe byte[] GetBytes()
		{
			//Update statistics
			int newPosition = Math.Min(Convert.ToInt32(Math.Log(_index, 2)) - base_size, _sizerStatistic.Length);
			if (newPosition < 0)
				newPosition = 0;

			_sizerStatistic[newPosition]++;

			byte[] result = new byte[_index];

			fixed (byte* source = _buffer)
			fixed (byte* dest = result)
				Buffer.MemoryCopy(source, dest, _index, _index);
			return result;

		}

		public void Write()
			=> Write((byte)ExportedDataTypes.none);

		public void WriteNull()
			=> Write((byte)ExportedDataTypes.@null);

		public void Write(bool value)
			=> Write((byte)(value ? 1 : 0), ExportedDataTypes.@bool);

		public unsafe void Write(sbyte value)
		{
			CheckAndResizeBuffer(sizeof(sbyte) + 1);
			_buffer[_index] = (byte)ExportedDataTypes.@sbyte;
			_index++;

			fixed (byte* ptr = _buffer)
				*(sbyte*)(ptr + _index) = value;
			_index++;
		}

		public void Write(byte value, ExportedDataTypes type = ExportedDataTypes.@byte)
		{
			CheckAndResizeBuffer(sizeof(byte) + 1);
			_buffer[_index] = (byte)type;
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
			=> Write(value.Ticks, ExportedDataTypes.dateTime);

		public void Write(TimeSpan value)
			=> Write(value.Ticks, ExportedDataTypes.dateTime);

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

		public void Write(IDictionary<string, object> map)
		{
			WriteMapHeader(map.Count);
			foreach (var pair in map)
			{
				WritePropertyName(pair.Key);
				Write(pair.Value);
			}
		}

		public void Write(IList<object> array)
		{
			WriteArrayHeader(array.Count);
			foreach (var value in array)
				Write(value);
		}

		private unsafe void WritePropertyName(string name)
		{
			fixed (char* source = name)
				Write(source, name.Length);
		}

		public void Write(object value)
			=> _delegates[(int)MdbTypeMap.GetTypeInfo(value.GetType()).MdbType](this, value);

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
