using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

namespace S031.MetaStack.Common
{
#if NO_COMMON
	internal class MapTable<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>
#else
	public class MapTable<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>
#endif
	{
		private const int default_capacity = 32;
		private const float golden_ratio = 1.618f;
		private const int collision_border = 17;
		private struct Entry
		{
			public int hashCode;    // Lower 31 bits of hash code, -1 if unused
			public int next;        // Index of next entry, -1 if last
			public TKey key;           // Key of entry
			public TValue value;         // Value of entry
		}

		private int[] _buckets;
		private Entry[] _entries;

		private int _count;
		private int _freeList;
		private int _freeCount;
		private int _collisionCount;

		private readonly IEqualityComparer<TKey> _comparer;
		private readonly object obj4Lock = new object();

		public MapTable() : this(default_capacity, null) { }

		public MapTable(int capacity) : this(capacity, null) { }

		public MapTable(IEqualityComparer<TKey> comparer) : this(default_capacity, comparer) { }

		public MapTable(int capacity, IEqualityComparer<TKey> comparer)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException(nameof(capacity));
			if (capacity > 0)
				Initialize(capacity);
			this._comparer = comparer ?? EqualityComparer<TKey>.Default;
		}

		public MapTable(IList<KeyValuePair<TKey, TValue>> dictionary, IEqualityComparer<TKey> comparer) :
			this(dictionary != null ? dictionary.Count : 0, comparer)
		{

			if (dictionary == null)
				throw new ArgumentNullException(nameof(dictionary));

			foreach (KeyValuePair<TKey, TValue> pair in dictionary)
				Insert(pair.Key, pair.Value, true, false);
		}

		public IEqualityComparer<TKey> Comparer => _comparer;

		public int Count => _count - _freeCount;

		public int Fragmentation => this._buckets.Count(i => i == -1) - _freeCount;

		public TValue this[TKey key]
		{
			get
			{
				int i = GetEntryIndex(key);
				if (i >= 0)
					return _entries[i].value;
				return default;
			}
			set => Insert(key, value, false);
		}

		public void Add(TKey key, TValue value) => Insert(key, value, true);

		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair) => Add(keyValuePair.Key, keyValuePair.Value);

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
			=> TryGetValue(keyValuePair.Key, out TValue value)
				&& EqualityComparer<TValue>.Default.Equals(value, keyValuePair.Value);

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
		{
			if (TryGetValue(keyValuePair.Key, out TValue value)
				&& EqualityComparer<TValue>.Default.Equals(value, keyValuePair.Value))
			{
				Remove(keyValuePair.Key);
				return true;
			}
			return false;
		}

		public void Clear()
		{
			if (_count > 0)
			{
				for (int i = 0; i < _buckets.Length; i++) _buckets[i] = -1;
				Array.Clear(_entries, 0, _count);
				_freeList = -1;
				_count = 0;
				_freeCount = 0;
			}
		}

		public bool ContainsKey(TKey key)
			=> GetEntryIndex(key) >= 0;

		public bool ContainsValue(TValue value)
		{
			if (value == null)
			{
				for (int i = 0; i < _count; i++)
				{
					if (_entries[i].hashCode >= 0 && _entries[i].value == null) return true;
				}
			}
			else
			{
				EqualityComparer<TValue> c = EqualityComparer<TValue>.Default;
				for (int i = 0; i < _count; i++)
				{
					if (_entries[i].hashCode >= 0 && c.Equals(_entries[i].value, value)) return true;
				}
			}
			return false;
		}

		private void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}

			if (index < 0 || index > array.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			if (array.Length - index < Count)
			{
				throw new ArgumentException(nameof(index));
			}

			int count = this._count;
			Entry[] entries = this._entries;
			for (int i = 0; i < count; i++)
			{
				if (entries[i].hashCode >= 0)
				{
					array[index++] = new KeyValuePair<TKey, TValue>(entries[i].key, entries[i].value);
				}
			}
		}

		public Enumerator GetEnumerator() => new Enumerator(this);

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => new Enumerator(this);

		IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

		private void Initialize(int capacity)
		{
			int size = capacity;
			int bSize = size;
			_buckets = new int[bSize];
			for (int i = 0; i < _buckets.Length; i++) _buckets[i] = -1;
			_entries = new Entry[size];
			_freeList = -1;
			_collisionCount = 0;
		}

		private void Insert(TKey key, TValue value, bool add, bool acquireLock = true)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			bool lockTaken = false;
			if (acquireLock)
				Monitor.Enter(obj4Lock, ref lockTaken);

			if (_buckets == null) Initialize(default_capacity);

			GetHashCodeAndBucketIndex(key, out int hashCode, out int bucketIndex);
			int index = GetEntryIndex(key, hashCode, bucketIndex);
			if (index >= 0)
			{
				if (add)
					throw new ArgumentException($"Key already exists {key}");
				_entries[index].value = value;
				return;
			}

			if (_freeCount > 0)
			{
				index = _freeList;
				_freeList = _entries[index].next;
				_freeCount--;
			}
			else
			{
				if (_count == _entries.Length)
				{
					Resize();
					bucketIndex = hashCode % _buckets.Length;
				}
				index = _count;
				_count++;
			}

			_entries[index].hashCode = hashCode;
			_entries[index].next = _buckets[bucketIndex];
			_entries[index].key = key;
			_entries[index].value = value;
			_buckets[bucketIndex] = index;
			if (lockTaken)
				Monitor.Exit(obj4Lock);
		}

		private void GetHashCodeAndBucketIndex(TKey key, out int hashCode, out int bucketIndex)
		{
			hashCode = _comparer.GetHashCode(key) & 0x7FFFFFFF;
			bucketIndex = hashCode % _buckets.Length;
		}

		private void Resize()
		{
			Resize(_count * 2, false);
		}

		private void Resize(int newSize, bool forceNewHashCodes)
		{
			int delta = Convert.ToInt32(Math.Pow(2, _collisionCount / collision_border));
			int bSize = _collisionCount > collision_border
				? _buckets.Length * delta
				//?Convert.ToInt32(newSize / golden_ratio)
				//:Convert.ToInt32(_buckets.Length * golden_ratio);
				: _buckets.Length * delta;
				//newSize < Math.Pow(2, _collisionCount + 10)
				//	? newSize * _collisionCount / 10
				//	: _buckets.Length * _collisionCount / 10;

			int[] newBuckets = new int[bSize];
			for (int i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;
			Entry[] newEntries = new Entry[newSize];
			Array.Copy(_entries, 0, newEntries, 0, _count);
			if (forceNewHashCodes)
			{
				for (int i = 0; i < _count; i++)
				{
					if (newEntries[i].hashCode != -1)
					{
						newEntries[i].hashCode = (_comparer.GetHashCode(newEntries[i].key) & 0x7FFFFFFF);
					}
				}
			}
			for (int i = 0; i < _count; i++)
			{
				if (newEntries[i].hashCode >= 0)
				{
					int bucket = newEntries[i].hashCode % bSize;
					newEntries[i].next = newBuckets[bucket];
					newBuckets[bucket] = i;
				}
			}
			_buckets = newBuckets;
			_entries = newEntries;
			_collisionCount = 0;
		}

		public bool Remove(TKey key)
		{
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}
			if (_buckets != null)
			{
				lock (obj4Lock)
				{
					int hashCode = _comparer.GetHashCode(key) & 0x7FFFFFFF;
					int bucket = hashCode % _buckets.Length;
					int last = -1;
					for (int i = _buckets[bucket]; i >= 0; last = i, i = _entries[i].next)
					{
						if (_entries[i].hashCode == hashCode && _comparer.Equals(_entries[i].key, key))
						{
							if (last < 0)
							{
								_buckets[bucket] = _entries[i].next;
							}
							else
							{
								_entries[last].next = _entries[i].next;
							}
							_entries[i].hashCode = -1;
							_entries[i].next = _freeList;
							_entries[i].key = default(TKey);
							_entries[i].value = default(TValue);
							_freeList = i;
							_freeCount++;
							return true;
						}
					}
				}
			}
			return false;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			int idx = GetEntryIndex(key);
			if (idx >= 0)
			{
				value = _entries[idx].value;
				return true;
			}
			value = default;
			return false;
		}

		private int GetEntryIndex(TKey key)
		{
			GetHashCodeAndBucketIndex(key, out int hashCode, out int bucketIndex);
			lock (obj4Lock)
				return GetEntryIndex(key, hashCode, bucketIndex);
		}
		private int GetEntryIndex(TKey key, int hashCode, int bucketIndex)
		{
			int collisionCount = 0;
			int result = -1;
			int i = _buckets[bucketIndex];
			for (; i >= 0;)
			{
				Entry entry = _entries[i];
				if (entry.hashCode == hashCode && _comparer.Equals(entry.key, key))
				{
					result = i;
					break;
				}
				i = entry.next;
				collisionCount++;
			}
			if (collisionCount > _collisionCount)
				_collisionCount = collisionCount;
			return result;
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
		{
			CopyTo(array, index);
		}

		public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
		{
			private readonly MapTable<TKey, TValue> _dictionary;
			private int _index;
			private KeyValuePair<TKey, TValue> _current;

			internal Enumerator(MapTable<TKey, TValue> dictionary)
			{
				this._dictionary = dictionary;
				_index = 0;
				_current = new KeyValuePair<TKey, TValue>();
			}

			public bool MoveNext()
			{
				// Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
				// dictionary.count+1 could be negative if dictionary.count is Int32.MaxValue
				while ((uint)_index < (uint)_dictionary._count)
				{
					if (_dictionary._entries[_index].hashCode >= 0)
					{
						_current = new KeyValuePair<TKey, TValue>(_dictionary._entries[_index].key, _dictionary._entries[_index].value);
						_index++;
						return true;
					}
					_index++;
				}

				_index = _dictionary._count + 1;
				_current = new KeyValuePair<TKey, TValue>();
				return false;
			}

			public KeyValuePair<TKey, TValue> Current => _current;

			public void Dispose()
			{
			}

			object IEnumerator.Current
			{
				get
				{
					if (_index == 0 || (_index == _dictionary._count + 1))
						throw new InvalidOperationException();

					return _current;
				}
			}

			void IEnumerator.Reset()
			{
				_index = 0;
				_current = new KeyValuePair<TKey, TValue>();
			}

		}
	}
}
