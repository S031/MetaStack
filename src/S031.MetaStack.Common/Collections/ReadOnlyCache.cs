using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace S031.MetaStack.Common
{
	/// <summary>
	/// Class for stor readonly array of data with binary search by key hash code
	/// smaller then readonly dictionary
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public readonly struct ReadOnlyCache<TKey, TValue>
	{
		private readonly TValue[] _data;
		private readonly int[] _keys;

		public TValue this[TKey index] =>
			_data[Array.BinarySearch<int>(_keys, index.GetHashCode())];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetValue(TKey key, out TValue value)
		{
			key.NullTest(nameof(key));
			int i = Array.BinarySearch<int>(_keys, key.GetHashCode());
			if (i >= 0)
			{
				value = _data[i];
				return true;
			}
			value = default;
			return false;
		}

		public bool Contains(TKey key)
			=> Array.BinarySearch<int>(_keys, key.GetHashCode()) >= 0;

		public ReadOnlyCache(params (TKey key, TValue value)[] data)
		{
			data.NullTest(nameof(data));
			int size = data.Length;
			_data = new TValue[size];
			_keys = new int[size];

			for (int i = 0; i < size; i++)
			{
				_data[i] = data[i].value;
				_keys[i] = data[i].key.GetHashCode();
			}
			Array.Sort(_keys, _data);
		}

	}
	public readonly struct ReadOnlyCache<TKey>
	{
		private readonly TKey[] _data;
		private readonly int[] _keys;

		public TKey this[TKey index] =>
			_data[Array.BinarySearch<int>(_keys, index.GetHashCode())];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetValue(TKey key, out TKey value)
		{
			int i = Array.BinarySearch<int>(_keys, key.GetHashCode());
			if (i >= 0)
			{
				value = _data[i];
				return true;
			}
			value = default;
			return false;
		}

		public bool Contains(TKey key)
			=> Array.BinarySearch<int>(_keys, key.GetHashCode()) >= 0;

		public ReadOnlyCache(params TKey[] data)
		{
			int size = data.Length;
			_data = new TKey[size];
			_keys = new int[size];

			for (int i = 0; i < size; i++)
			{
				_data[i] = data[i];
				_keys[i] = data[i].GetHashCode();
			}
			Array.Sort(_keys, _data);
		}

	}
}
