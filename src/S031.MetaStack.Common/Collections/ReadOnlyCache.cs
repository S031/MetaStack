using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S031.MetaStack.Common
{
	/// <summary>
	/// Class for stor readonly array of data with binary search by key hesh code
	/// fastest and smaller then readonly dictionary
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public sealed class ReadOnlyCache<TKey, TValue> : IEnumerable<Pair<TKey, TValue>>
	{
		private Pair<int, TValue>[] _data;
		private int _free;
		private int _size;

		private static readonly Comparer<int> _comparer = Comparer<int>.Default;

		private int BinarySearch(int searchFor)
		{
			var array = _data;
			int high = _size - 1;
			int low = 0;
			int mid;

			while (low <= high)
			{
				mid = (high + low) / 2;
				int result = _comparer.Compare(array[mid].Key, searchFor);
				if (result == 0)
					return mid;
				else if (result > 0)
					high = mid--;
				else
					low = mid++;
			}
			return -1;
		}

		public TValue this[TKey index] =>
			_data[BinarySearch(index.GetHashCode())].Value;

		public bool TryGetValue(TKey key, out TValue value)
		{
			int i = BinarySearch(key.GetHashCode());
			if (i > 0)
			{
				value = _data[i].Value;
				return true;
			}
			value = default;
			return false;
		}

		public ReadOnlyCache(int size)
		{
			_data = new Pair<int, TValue>[size];
			_free = size;
			_size = size;
		}

		public IEnumerator<Pair<TKey, TValue>> GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator<Pair<TKey, TValue>>)_data.GetEnumerator();
		}

		public ReadOnlyCache<TKey, TValue> Add(TKey key, TValue value)
		{
			int index = _size - _free;
			_data[index] = new Pair<int, TValue>(key.GetHashCode(), value);
			_free--;
			return this;
		}

		public ReadOnlyCache<TKey, TValue> Sort()
		{
			_size -= _free;
			_free = 0;
			_data = _data
				.Take(_size)
				.OrderBy(p => p.Key)
				.ToArray();
			return this;
		}
	}
	public readonly struct Pair<TKey, TValue>
	{
		public readonly TKey Key;
		public readonly TValue Value;

		public Pair(TKey key, TValue value)
		{
			Key = key;
			Value = value;
		}
	}
}
