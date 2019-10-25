using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace S031.MetaStack.Common
{
	public class MapTable<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>
	{
		private const int default_capacity = 32;
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
				Add(pair.Key, pair.Value);
		}

		public IEqualityComparer<TKey> Comparer => _comparer;

		public int Count => _count - _freeCount;

		public TValue this[TKey key]
		{
			get
			{
				int i = FindEntry(key);
				if (i >= 0)
					return _entries[i].value;
				return default(TValue);
			}
			set => Insert(key, value, false);
		}

		public void Add(TKey key, TValue value) => Insert(key, value, true);

		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair) => Add(keyValuePair.Key, keyValuePair.Value);

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
		{
			int i = FindEntry(keyValuePair.Key);
			if (i >= 0 && EqualityComparer<TValue>.Default.Equals(_entries[i].value, keyValuePair.Value))
			{
				return true;
			}
			return false;
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
		{
			int i = FindEntry(keyValuePair.Key);
			if (i >= 0 && EqualityComparer<TValue>.Default.Equals(_entries[i].value, keyValuePair.Value))
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

		public bool ContainsKey(TKey key) => FindEntry(key) >= 0;

		public int IndexOf(TKey key) => FindEntry(key);

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

		public Enumerator GetEnumerator() => new Enumerator(this, Enumerator.KeyValuePair);

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => new Enumerator(this, Enumerator.KeyValuePair);

		IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this, Enumerator.KeyValuePair);

		private int FindEntry(TKey key)
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
					for (int i = _buckets[hashCode % _buckets.Length]; i >= 0; i = _entries[i].next)
					{
						if (_entries[i].hashCode == hashCode && _comparer.Equals(_entries[i].key, key)) return i;
					}
				}
			}
			return -1;
		}

		private void Initialize(int capacity)
		{
			int size = capacity;
			_buckets = new int[size];
			for (int i = 0; i < _buckets.Length; i++) _buckets[i] = -1;
			_entries = new Entry[size];
			_freeList = -1;
		}

		private void Insert(TKey key, TValue value, bool add)
		{

			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			lock (obj4Lock)
			{
				if (_buckets == null) Initialize(default_capacity);
				int hashCode = _comparer.GetHashCode(key) & 0x7FFFFFFF;
				int targetBucket = hashCode % _buckets.Length;

				for (int i = _buckets[targetBucket]; i >= 0; i = _entries[i].next)
				{
					if (_entries[i].hashCode == hashCode && _comparer.Equals(_entries[i].key, key))
					{
						if (add)
						{
							throw new ArgumentException(nameof(key));
						}
						_entries[i].value = value;
						return;
					}
				}
				int index;
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
						targetBucket = hashCode % _buckets.Length;
					}
					index = _count;
					_count++;
				}

				_entries[index].hashCode = hashCode;
				_entries[index].next = _buckets[targetBucket];
				_entries[index].key = key;
				_entries[index].value = value;
				_buckets[targetBucket] = index;
			}
		}

		private void Resize()
		{
			Resize(_count, false);
		}

		private void Resize(int newSize, bool forceNewHashCodes)
		{
			Contract.Assert(newSize >= _entries.Length);
			int[] newBuckets = new int[newSize];
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
					int bucket = newEntries[i].hashCode % newSize;
					newEntries[i].next = newBuckets[bucket];
					newBuckets[bucket] = i;
				}
			}
			_buckets = newBuckets;
			_entries = newEntries;
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
			int i = FindEntry(key);
			if (i >= 0)
			{
				value = _entries[i].value;
				return true;
			}
			value = default(TValue);
			return false;
		}

		internal TValue GetValueOrDefault(TKey key)
		{
			int i = FindEntry(key);
			if (i >= 0)
			{
				return _entries[i].value;
			}
			return default(TValue);
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
		{
			CopyTo(array, index);
		}

		public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>,
			IDictionaryEnumerator
		{
			private readonly MapTable<TKey, TValue> dictionary;
			private int index;
			private KeyValuePair<TKey, TValue> current;
			private readonly int getEnumeratorRetType;  // What should Enumerator.Current return?

			internal const int DictEntry = 1;
			internal const int KeyValuePair = 2;

			internal Enumerator(MapTable<TKey, TValue> dictionary, int getEnumeratorRetType)
			{
				this.dictionary = dictionary;
				index = 0;
				this.getEnumeratorRetType = getEnumeratorRetType;
				current = new KeyValuePair<TKey, TValue>();
			}

			public bool MoveNext()
			{
				// Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
				// dictionary.count+1 could be negative if dictionary.count is Int32.MaxValue
				while ((uint)index < (uint)dictionary._count)
				{
					if (dictionary._entries[index].hashCode >= 0)
					{
						current = new KeyValuePair<TKey, TValue>(dictionary._entries[index].key, dictionary._entries[index].value);
						index++;
						return true;
					}
					index++;
				}

				index = dictionary._count + 1;
				current = new KeyValuePair<TKey, TValue>();
				return false;
			}

			public KeyValuePair<TKey, TValue> Current => current;

			public void Dispose()
			{
			}

			object IEnumerator.Current
			{
				get
				{
					if (index == 0 || (index == dictionary._count + 1))
					{
						throw new InvalidOperationException();
					}

					if (getEnumeratorRetType == DictEntry)
					{
						return new System.Collections.DictionaryEntry(current.Key, current.Value);
					}
					else
					{
						return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
					}
				}
			}

			void IEnumerator.Reset()
			{
				index = 0;
				current = new KeyValuePair<TKey, TValue>();
			}

			DictionaryEntry IDictionaryEnumerator.Entry => new DictionaryEntry(current.Key, current.Value);

			object IDictionaryEnumerator.Key => current.Key;

			object IDictionaryEnumerator.Value => current.Value;
		}
	}
}
