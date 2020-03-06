using System;
#if NETCOREAPP
using System.Buffers;
#endif
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace S031.MetaStack.Buffers
{
	/// <summary>
	/// !!!Make it disposable
	/// </summary>
	public class BinaryDataBuffer: IDisposable
	{
#if NETCOREAPP
		private static readonly ArrayPool<byte> _pool = ArrayPool<byte>.Shared;
#endif
		byte[] _buffer;
		int _dataSize;
		int _index;
		readonly int _start;
		readonly bool _isReadOnly;

		private const int base_size = 4;
		private static readonly int[] _sizerStatistic = new int[32];

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

		public unsafe BinaryDataBuffer(int capacity)
		{
			capacity = GetCapacity(capacity);
#if NETCOREAPP
			_buffer = _pool.Rent(capacity);
#else
			_buffer = new byte[capacity];
#endif
			_start = 0;
			_index = 0;
			_dataSize = 0;
			_isReadOnly = false;
		}

		internal BinaryDataBuffer(byte[] source, bool readOnly = false)
		{
			_buffer = source;
			_start = 0;
			_index = 0;
			_dataSize = source.Length;
			_isReadOnly = readOnly;
		}

		internal BinaryDataBuffer(byte[] source, int start = 0, int lenght = 0, bool readOnly = true)
		{
			_buffer = source;
			if (lenght == 0)
				lenght = source.Length;

			if (start >= source.Length)
				throw new IndexOutOfRangeException();

			_start = start;
			_index = 0;
			_dataSize = lenght;
			_isReadOnly = readOnly || start != 0 || lenght != source.Length;
		}

		internal unsafe byte* this[int index]
		{
			get
			{
				fixed (byte* ptr = &_buffer[_start + index])
					return ptr;
			}
		}

		internal unsafe byte* Ref
		{
			get
			{
				fixed (byte* ptr = &_buffer[_start + _index])
					return ptr;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Skip() => _index++;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Skip(int count) => (_index += count);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void CheckAndResizeBuffer(int sizeHint)
		{
			if (_isReadOnly)
				throw new InvalidOperationException($"{nameof(BinaryDataBuffer)} is read only");

			//for not read only start == 0 forever
			_dataSize = _index + sizeHint;

			if (_dataSize > _buffer.Length)
			{
				int growBy = Math.Max(sizeHint, _buffer.Length);

				int newSize = checked(_buffer.Length + growBy);

				byte[] oldBuffer = _buffer;
#if NETCOREAPP
				_buffer = _pool.Rent(newSize);
#else
				_buffer = new byte[newSize];
#endif

				fixed (byte* source = oldBuffer)
				fixed (byte* dest = _buffer)
					Buffer.MemoryCopy(source, dest, newSize, _index);
#if NETCOREAPP
				_pool.Return(oldBuffer);
#endif
			}
		}

		public int Position
		{
			get { return _index; }
			set
			{
				if (value < 0 || value > _dataSize)
					throw new ArgumentOutOfRangeException(nameof(value));
				_index = value;
			}
		}

		public int Length
			=> _dataSize;

		internal byte[] Source
			=> _buffer;

		public unsafe byte[] GetBytes()
		{
			//Update statistics
			int newPosition = Math.Min(Convert.ToInt32(Math.Log(_dataSize, 2)) - base_size, _sizerStatistic.Length);
			if (newPosition < 0)
				newPosition = 0;

			_sizerStatistic[newPosition]++;

			byte[] result = new byte[_dataSize];

			fixed (byte* source = &_buffer[_start])
			fixed (byte* dest = result)
				Buffer.MemoryCopy(source, dest, _dataSize, _dataSize);
			return result;

		}

		public BinaryDataBuffer Slice(int startPosition, int lenght)
			=> new BinaryDataBuffer(_buffer, _start + startPosition, lenght, true);

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
#if NETCOREAPP
				_pool.Return(_buffer);
#else
				GC.Collect();
#endif
			}
		}

		public static implicit operator byte[](BinaryDataBuffer b) 
			=> b.Source;

		public static explicit operator BinaryDataBuffer(byte[] b) 
			=> new BinaryDataBuffer(b, false);
	}
}
