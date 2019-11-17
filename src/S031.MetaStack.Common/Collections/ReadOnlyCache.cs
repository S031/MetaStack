using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace S031.MetaStack.Common
{
	/// <summary>
	/// Class for stor readonly array of data with binary search by key hash code
	/// smaller then readonly dictionary && no memory fragmentation
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
#if NO_COMMON
	internal readonly struct ReadOnlyCache<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
#else
	public readonly struct ReadOnlyCache<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
#endif
	{
		private readonly TValue[] _data;
		private readonly int[] _keys;
		private readonly IEqualityComparer<TKey> _comparer;

		public IEnumerable<TKey> Keys => throw new NotImplementedException();

		public IEnumerable<TValue> Values => _data;

		public int Count => _data.Length;

		public TValue this[TKey index] =>
			_data[Array.BinarySearch<int>(_keys, _comparer.GetHashCode(index))];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetValue(TKey key, out TValue value)
		{
			key.NullTest(nameof(key));
			int i = Array.BinarySearch<int>(_keys, _comparer.GetHashCode(key));
			if (i >= 0)
			{
				value = _data[i];
				return true;
			}
			value = default;
			return false;
		}

		public bool Contains(TKey key)
			=> Array.BinarySearch<int>(_keys, _comparer.GetHashCode(key)) >= 0;

		public bool ContainsKey(TKey key) => Contains(key);

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()=>
			throw new NotImplementedException();

		IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();

		public ReadOnlyCache(params (TKey key, TValue value)[] data) : this(null, data) { }
		
		public ReadOnlyCache(IEqualityComparer<TKey> comparer = null, params (TKey key, TValue value)[] data)
		{
			data.NullTest(nameof(data));
			_comparer = comparer ?? EqualityComparer<TKey>.Default;
			int size = data.Length;
			_data = new TValue[size];
			_keys = new int[size];

			for (int i = 0; i < size; i++)
			{
				_data[i] = data[i].value;
				_keys[i] = _comparer.GetHashCode(data[i].key);
			}
			Array.Sort(_keys, _data);
		}

		public ReadOnlyCache(IList<KeyValuePair<TKey, TValue>> data)
		{
			data.NullTest(nameof(data));
			_comparer = EqualityComparer<TKey>.Default;
			int size = data.Count;
			_data = new TValue[size];
			_keys = new int[size];

			for (int i = 0; i < size; i++)
			{
				_data[i] = data[i].Value;
				_keys[i] = _comparer.GetHashCode(data[i].Key);
			}
			Array.Sort(_keys, _data);
		}

	}
	public readonly struct ReadOnlyCache<TKey>
	{
		private readonly TKey[] _data;
		private readonly int[] _keys;
		private readonly IEqualityComparer<TKey> _comparer;

		public TKey this[TKey index] =>
			_data[Array.BinarySearch<int>(_keys, _comparer.GetHashCode(index))];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetValue(TKey key, out TKey value)
		{
			int i = Array.BinarySearch<int>(_keys, _comparer.GetHashCode(key));
			if (i >= 0)
			{
				value = _data[i];
				return true;
			}
			value = default;
			return false;
		}

		public bool Contains(TKey key)
			=> Array.BinarySearch<int>(_keys, _comparer.GetHashCode(key)) >= 0;

		public ReadOnlyCache(params TKey[] data)
		{
			_comparer = EqualityComparer<TKey>.Default;
			int size = data.Length;
			_data = new TKey[size];
			_keys = new int[size];

			for (int i = 0; i < size; i++)
			{
				_data[i] = data[i];
				_keys[i] = _comparer.GetHashCode(data[i]);
			}
			Array.Sort(_keys, _data);
		}
	}

#if NO_COMMON
	internal static class ObjectExtensions
	{
		internal static void NullTest(this object value, string nameOf)
		{
			if (value == null) throw new ArgumentNullException(nameOf);
		}
	}
#endif
}
