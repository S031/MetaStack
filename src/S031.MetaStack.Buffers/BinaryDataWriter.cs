using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Buffers
{
	/// <summary>
	/// Inherited from <see cref="TypeCode"/> 
	/// </summary>
	public enum ExportedDataTypes : byte
	{
		@none = TypeCode.Empty,
		@object = TypeCode.Object,
		@null = TypeCode.DBNull,
		@bool = TypeCode.Boolean,
		@char = TypeCode.Char,
		@sbyte = TypeCode.SByte,
		@byte = TypeCode.Byte,
		@short = TypeCode.Int16,
		@ushort = TypeCode.UInt16,
		@int = TypeCode.Int32,
		@uint = TypeCode.UInt32,
		@long = TypeCode.Int64,
		@ulong = TypeCode.UInt64,
		@float = TypeCode.Single,
		@double = TypeCode.Double,
		@decimal = TypeCode.Decimal,
		dateTime = TypeCode.DateTime,
		@guid = TypeCode.String - 1,
		@string = TypeCode.String,
		@byteArray = TypeCode.String + 1,
		@asciiString = TypeCode.String + 2,
		@utf8String = TypeCode.String + 3,
		@array = TypeCode.String + 4,
		@serial = TypeCode.String + 5
	}


	public class BinaryDataWriter
	{
		unsafe static readonly Action<BinaryDataWriter, object>[] _delegates = new Action<BinaryDataWriter, object>[]
		{
			(writer, value)=>writer.Write(),
			(writer, value)=>writer.Write((IDictionary<string, object>)value),
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
			(writer, value) => writer.Write((IList<object>)value),
			(writer, value) => writer.WriteWithFormatter(value)
		};

		internal static ExportedDataTypes GetExportedType(Type t)
		{
			TypeCode tcode = System.Type.GetTypeCode(t);

			if (tcode == TypeCode.Object)
			{
				if (t == typeof(byte[]))
					return ExportedDataTypes.byteArray;
				else if (t == typeof(Guid))
					return ExportedDataTypes.guid;
				else if (typeof(IDictionary<string, object>).IsAssignableFrom(t))
					return ExportedDataTypes.@object;
				else if (typeof(IList<object>).IsAssignableFrom(t))
					return ExportedDataTypes.array;
				else
					return ExportedDataTypes.serial;
			}
			return (ExportedDataTypes)tcode;
		}

		readonly BinaryDataBuffer _buffer;
		public BinaryDataWriter(BinaryDataBuffer buffer)
		{
			_buffer = buffer;
		}

		public BinaryDataWriter(int capacity) 
			: this(new BinaryDataBuffer(capacity))
		{
		}

		internal BinaryDataWriter(byte[] source)
			: this(new BinaryDataBuffer(source, 0, source.Length, false))
		{
		}

		public int Position
		{
			get { return _buffer.Position; }
			set { _buffer.Position = value; }
		}

		public int Length
			=> _buffer.Length;

		internal byte[] Source
			=> _buffer.Source;

		public unsafe byte[] GetBytes()
			=> _buffer.GetBytes();

		public unsafe BinaryDataWriter Write(ExportedDataTypes type)
		{
			CheckAndResizeBuffer(sizeof(byte));
			*(byte*)_buffer.Ref = (byte)type;
			_buffer.Skip();
			return this;
		}

		public BinaryDataWriter Write()
			=> Write(ExportedDataTypes.none);

		public BinaryDataWriter WriteNull()
			=> Write(ExportedDataTypes.@null);

		public BinaryDataWriter WriteSpace(int count)
		{
			_buffer.CheckAndResizeBuffer(count);
			_buffer.Skip(count);
			return this;
		}

		public BinaryDataWriter Write(bool value)
			=> Write((byte)(value ? 1 : 0), ExportedDataTypes.@bool);

		public unsafe BinaryDataWriter Write(sbyte value)
		{
			CheckAndResizeBuffer(sizeof(sbyte) + 1);
			*(byte*)_buffer.Ref = (byte)ExportedDataTypes.@sbyte;
			_buffer.Skip();			

			*(sbyte*)_buffer.Ref = value;
			_buffer.Skip();
			return this;
		}

		public unsafe BinaryDataWriter Write(byte value, ExportedDataTypes type = ExportedDataTypes.@byte)
		{
			CheckAndResizeBuffer(sizeof(byte) + 1);
			*(byte*)_buffer.Ref = (byte)type;
			_buffer.Skip();

			*(byte*)_buffer.Ref = value;
			_buffer.Skip();
			return this;
		}

		public unsafe BinaryDataWriter Write(short value, ExportedDataTypes type = ExportedDataTypes.@short)
		{
			CheckAndResizeBuffer(sizeof(short) + 1);
			*(byte*)_buffer.Ref = (byte)type;
			_buffer.Skip();

			*(short*)_buffer.Ref = value;
			_buffer.Skip(sizeof(short));
			return this;
		}

		public BinaryDataWriter Write(ushort value)
			=> Write((short)value, ExportedDataTypes.@ushort);

		public unsafe BinaryDataWriter Write(int value, ExportedDataTypes type = ExportedDataTypes.@int)
		{
			CheckAndResizeBuffer(sizeof(int) + 1);
			*(byte*)_buffer.Ref = (byte)type;
			_buffer.Skip();

			*(int*)_buffer.Ref = value;
			_buffer.Skip(sizeof(int));
			return this;
		}

		public unsafe BinaryDataWriter Write(uint value)
			=> Write((int)value, ExportedDataTypes.@uint);

		public unsafe BinaryDataWriter Write(long value, ExportedDataTypes type = ExportedDataTypes.@long)
		{
			CheckAndResizeBuffer(sizeof(long) + 1);
			*(byte*)_buffer.Ref = (byte)type;
			_buffer.Skip();

			*(long*)_buffer.Ref = value;
			_buffer.Skip(sizeof(long));
			return this;
		}

		public unsafe BinaryDataWriter Write(decimal value)
		{
			CheckAndResizeBuffer(sizeof(decimal) + 1);
			*(byte*)_buffer.Ref = (byte)ExportedDataTypes.@decimal;
			_buffer.Skip();

			*(decimal*)_buffer.Ref = value;
			_buffer.Skip(sizeof(decimal));
			return this;
		}

		public BinaryDataWriter Write(ulong value)
			=> Write((long)value, ExportedDataTypes.@ulong);

		public unsafe BinaryDataWriter Write(float value)
			=> Write(*(int*)&value, ExportedDataTypes.@float);

		public unsafe BinaryDataWriter Write(double value)
			=> Write(*(long*)&value, ExportedDataTypes.@double);

		public BinaryDataWriter Write(DateTime value)
			=> Write(value.Ticks, ExportedDataTypes.dateTime);

		public BinaryDataWriter Write(TimeSpan value)
			=> Write(value.Ticks, ExportedDataTypes.dateTime);

		public unsafe BinaryDataWriter Write(char[] value)
		{
			var encoding = Encoding.UTF8;

			byte[] data = encoding.GetBytes(value);
			int size = data.Length;

			CheckAndResizeBuffer(size + sizeof(int) + 1);
			*(byte*)_buffer.Ref = (byte)ExportedDataTypes.utf8String;
			_buffer.Skip();

			*(int*)_buffer.Ref = size;
			_buffer.Skip(sizeof(int));

			fixed (byte* source = data)
				Buffer.MemoryCopy(source, _buffer.Ref, size, size);
			_buffer.Skip(size);
			return this;
		}

		/// <summary>
		/// Write string in two byte array
		/// </summary>
		/// <param name="value"></param>
		public unsafe BinaryDataWriter Write(string value)
		{
			int size = value.Length * sizeof(char);
			CheckAndResizeBuffer(size + sizeof(int) + 1);

			*(byte*)_buffer.Ref = (byte)ExportedDataTypes.@string;
			_buffer.Skip();

			*(int*)_buffer.Ref = size;
			_buffer.Skip(sizeof(int));

			if (size > 0)
			{
				fixed (char* source = value)
					Buffer.MemoryCopy(source, _buffer.Ref, size, size);
				_buffer.Skip(size);
			}
			return this;
		}

		/// <summary>
		/// Write string to one byte array (for ascii string symbols only)
		/// </summary>
		/// <param name="value"></param>
		/// <param name="size"></param>
		public unsafe BinaryDataWriter Write(char* value, int size)
		{
			CheckAndResizeBuffer(size + sizeof(int) + 1);
			*(byte*)_buffer.Ref = (byte)ExportedDataTypes.asciiString;
			_buffer.Skip();

			*(int*)_buffer.Ref = size;
			_buffer.Skip(sizeof(int));

			if (size > 0)
			{
				byte* dest = _buffer.Ref;
				for (int i = 0; i < size; i++)
					dest[i] = (byte)value[i];
				_buffer.Skip(size);
			}
			return this;
		}

		public unsafe BinaryDataWriter Write(byte[] value)
		{
			int size = value.Length;
			CheckAndResizeBuffer(size + sizeof(int) + 1);

			*(byte*)_buffer.Ref = (byte)ExportedDataTypes.byteArray;
			_buffer.Skip();

			*(int*)_buffer.Ref = size;
			_buffer.Skip(sizeof(int));

			if (size > 0)
			{
				fixed (byte* source = value)
					Buffer.MemoryCopy(source, _buffer.Ref, size, size);
				_buffer.Skip(size);
			}
			return this;
		}

		public unsafe BinaryDataWriter Write(Guid value)
		{
			const int size = 16;
			CheckAndResizeBuffer(size + 1);

			*(byte*)_buffer.Ref = (byte)ExportedDataTypes.guid;
			_buffer.Skip();

			fixed (byte* source = value.ToByteArray())
				Buffer.MemoryCopy(source, _buffer.Ref, size, size);
			_buffer.Skip(size);
			return this;
		}

		public unsafe BinaryDataWriter WriteMapHeader(int count)
			=> Write(count, ExportedDataTypes.@object);

		public unsafe BinaryDataWriter WriteArrayHeader(int count)
			=> Write(count, ExportedDataTypes.@array);

		public BinaryDataWriter Write(IDictionary<string, object> map)
		{
			WriteMapHeader(map.Count);
			foreach (var pair in map)
			{
				WritePropertyName(pair.Key);
				Write(pair.Value);
			}
			return this;
		}

		public BinaryDataWriter Write(IList<object> array)
		{
			WriteArrayHeader(array.Count);
			foreach (var value in array)
				Write(value);
			return this;
		}

		private unsafe void WritePropertyName(string name)
		{
			fixed (char* source = name)
				Write(source, name.Length);
		}

		public BinaryDataWriter Write(object value)
		{
			if (value == null)
				Write();
			else
				_delegates[(int)GetExportedType(value.GetType())](this, value);
			return this;
		}

		private BinaryDataWriter WriteWithFormatter(object value)
		{
			Type t = value.GetType();
			if (!BinaryDataFormatterService.Resolve(t, out IBinaryDataFormatter f))
				throw new InvalidOperationException($"Can't write {t} as binary data, formatter not found");

			Write(ExportedDataTypes.serial);
			WritePropertyName(t.AssemblyQualifiedName);
			f.Write(this, value);
			return this;
		}

		private unsafe void CheckAndResizeBuffer(int sizeHint)
			=> _buffer.CheckAndResizeBuffer(sizeHint);
	}
}
