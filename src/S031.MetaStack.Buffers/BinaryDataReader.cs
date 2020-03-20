using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace S031.MetaStack.Buffers
{
	public class BinaryDataReader
	{
		readonly BinaryDataBuffer _buffer;
		public BinaryDataReader(BinaryDataBuffer buffer)
		{
			_buffer = buffer;
		}

		internal BinaryDataReader(byte[] source)
			: this(new BinaryDataBuffer(source, 0, source.Length, true))
		{
		}

		internal byte[] Source
			=> _buffer.Source;

		public int Lenght
			=> _buffer.Length;

		public int Position
		{
			get { return _buffer.Position; }
			set { _buffer.Position = value; }
		}

		public bool EOF
			=> _buffer.Position >= _buffer.Length;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe ExportedDataTypes ReadNext()
		{
			if (EOF)
				return ExportedDataTypes.none;
			
			var result = (ExportedDataTypes)(*_buffer.Ref);
			_buffer.Skip();
			return result;
		}

		public unsafe string ReadString()
		{
			if (EOF)
				throw new IndexOutOfRangeException();

			int size = ReadInt32();
			if (_buffer.Position + size > _buffer.Length)
				throw new IndexOutOfRangeException();

			if (size > 0)
			{
				fixed (char* dest = new char[size])
				{
					Buffer.MemoryCopy(_buffer.Ref, dest, size, size);
					_buffer.Skip(size);
					return new string(dest);
				};
			}
			return string.Empty;
		}

		public unsafe string ReadAsciiString()
		{
			if (EOF)
				throw new IndexOutOfRangeException();

			int size = ReadInt32();
			if (_buffer.Position + size > _buffer.Length)
				throw new IndexOutOfRangeException();

			if (size > 0)
			{
				byte* source = _buffer.Ref;
				char[] dest = new char[size];

				for (int i = 0; i < size; i++)
					dest[i] = (char)source[i];

				_buffer.Skip(size);
				return new string(dest);
			}
			return string.Empty;
		}

		public unsafe string ReadUtf8String()
		{
			if (EOF)
				throw new IndexOutOfRangeException();

			int size = ReadInt32();
			if (_buffer.Position + size > _buffer.Length)
				throw new IndexOutOfRangeException();

			if (size > 0)
			{
				_buffer.Skip(size);
				return Encoding.UTF8.GetString(_buffer.Ref, size);
			}
			return string.Empty;
		}

		public bool ReadBool()
			=> ReadByte() == 0 ? false : true;

		public unsafe Byte ReadByte()
		{
			byte result = *_buffer.Ref;
			_buffer.Skip();
			return result;
		}

		public unsafe byte[] ReadBytes()
		{
			if (EOF)
				throw new IndexOutOfRangeException();

			int size = ReadInt32();
			if (_buffer.Position + size > _buffer.Length)
				throw new IndexOutOfRangeException();

			if (size > 0)
			{
				byte[] data = new byte[size];
				fixed (byte* dest = data)
					Buffer.MemoryCopy(_buffer.Ref, dest, size, size);
				_buffer.Skip(size);
				return data;
			}
			return new byte[] { };
		}

		public unsafe sbyte ReadSByte()
		{
			sbyte result = *(sbyte*)_buffer.Ref;
			_buffer.Skip(sizeof(sbyte));
			return result;
		}

		public unsafe short ReadInt16()
		{
			short result = *(short*)_buffer.Ref;
			_buffer.Skip(sizeof(short));
			return result;
		}
		
		public unsafe ushort ReadUInt16()
		{
			ushort result =*(ushort*)_buffer.Ref;
			_buffer.Skip(sizeof(ushort));
			return result;
		}
		
		public unsafe int ReadInt32()
		{
			int result = *(int*)_buffer.Ref;
			//int result = BitConverter.ToInt32(_buffer, 1);
			_buffer.Skip(sizeof(int));
			return result;
		}
		
		public unsafe uint ReadUInt32()
		{
			uint result = *(uint*)_buffer.Ref;
			_buffer.Skip(sizeof(uint));
			return result;
		}

		public unsafe long ReadInt64()
		{
			long result = *(long*)_buffer.Ref;
			_buffer.Skip(sizeof(long));
			return result;
		}

		public unsafe ulong ReadUInt64()
		{
			ulong result = *(ulong*)_buffer.Ref;
			_buffer.Skip(sizeof(ulong));
			return result;
		}

		public unsafe float ReadSingle()
		{
			float result = *(float*)_buffer.Ref;
			_buffer.Skip(sizeof(float));
			return result;
		}

		public unsafe double ReadDouble()
		{
			double result = *(double*)_buffer.Ref;
			_buffer.Skip(sizeof(double));
			return result;
		}

		public unsafe decimal ReadDecimal()
		{
			decimal result = *(decimal*)_buffer.Ref;
			_buffer.Skip(sizeof(decimal));
			return result;
		}

		public DateTime ReadDate()
			=> new DateTime(ReadInt64());
		
		public unsafe Guid ReadGuid()
		{
			const int guid_len = 16;

			byte[] data = new byte[guid_len];
			fixed (byte* dest =  data)
				Buffer.MemoryCopy(_buffer.Ref, dest, guid_len, guid_len);

			Guid result = new Guid(data);
			_buffer.Skip(guid_len);
			return result;
		}

		public void ReadRaw(IDictionary<string, object> map)
		{
			int count = Read<int>();
			for (int i = 0; i < count; i++)
				map[ReadPropertyName()] = ReadValue();
		}

		private string ReadPropertyName()
		{
			var t = ReadNext();
			if (t != ExportedDataTypes.asciiString)
				throw new FormatException("Expected property name string");

			string propertyName = ReadAsciiString();
			if (string.IsNullOrEmpty(propertyName))
				throw new FormatException("Property name string must have a value");
			return propertyName;
		}

		private void ReadArrayRaw(IList<object> array)
		{
			int count = ReadInt32();
			for (int i = 0; i < count; i++)
				array.Add(ReadValue());
		}

		static readonly Func<BinaryDataReader, object>[] _delegates = new Func<BinaryDataReader, object>[]
		{
			(reader)=>null,
			(reader)=>
			{
					MapTable<string, object> map = new MapTable<string, object>();
					reader.ReadRaw(map);
					return map;
			},
			(reader)=>DBNull.Value,
			(reader)=>reader.ReadBool(),
			(reader)=>(char)reader.ReadInt16(),
			(reader)=>reader.ReadSByte(),
			(reader)=>reader.ReadByte(),
			(reader)=>reader.ReadInt16(),
			(reader)=>reader.ReadUInt16(),
			(reader)=>reader.ReadInt32(),
			(reader)=>reader.ReadUInt32(),
			(reader)=>reader.ReadUInt64(),
			(reader)=>reader.ReadUInt64(),
			(reader)=>reader.ReadSingle(),
			(reader)=>reader.ReadDouble(),
			(reader)=>reader.ReadDecimal(),
			(reader)=>reader.ReadDate(),
			(reader)=>reader.ReadGuid(),
			(reader)=>reader.ReadString(),
			(reader)=>reader.ReadBytes(),
			(reader) =>reader.ReadAsciiString(),
			(reader) => reader.ReadUtf8String(),
			(reader) => 
			{
				List<object> a = new List<object>();
				reader.ReadArrayRaw(a);
				return a;
			},
			(reader) => reader.ReadWithFormatter()
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public object ReadValue()
			=> _delegates[(int)ReadNext()](this);

		public object[] ReadValues(int count)
		{
			object[] result = new object[count];
			for (int i = 0; i<count;i++)
				result[i] = _delegates[(int)ReadNext()](this);
			return result;
		}

#if NETCOREAPP
		//public unsafe T Read<T>()
		//	where T : unmanaged
		//{
		//	ReadNext();
		//	T result = *(T*)_buffer.Ref;
		//	_buffer.Skip(sizeof(T));
		//	return result;
		//}
#endif
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Read<T>()
			=> (T)_delegates[(int)ReadNext()](this);

		private object ReadWithFormatter()
		{
			string typeName = ReadPropertyName();
			var t = TypeExtensions.GetTypeByAssemblyQualifiedName(typeName);

			IBinaryDataFormatter f;
			if (!BinaryDataFormatterService.Resolve(t, out f))
				throw new InvalidOperationException($"Can't read {typeName} from binary data, formatter not found");
			return f.Read(t, this);
		}
	}

}
