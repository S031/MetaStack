using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S031.MetaStack.Common
{
	/// <summary>
	/// Class for stor readonly array of data with binary search by key hash code
	/// fastest and smaller then readonly dictionary
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public readonly struct ReadOnlyCache<TKey, TValue>
	{
		private readonly TValue[] _data;
		private readonly int[] _keys;

		public TValue this[TKey index] =>
			_data[Array.BinarySearch<int>(_keys, index.GetHashCode())];

		public bool TryGetValue(TKey key, out TValue value)
		{
			int i = Array.BinarySearch<int>(_keys, key.GetHashCode());
			if (i > 0)
			{
				value = _data[i];
				return true;
			}
			value = default;
			return false;
		}

		public ReadOnlyCache(params (TKey key, TValue value)[] data)
		{
			_data = data.Select(p => p.value).ToArray();
			_keys = data.Select(p => p.key.GetHashCode()).ToArray();
			Array.Sort(_keys, _data);
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
