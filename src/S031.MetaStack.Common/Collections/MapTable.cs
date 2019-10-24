using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace S031.MetaStack.Common
{
	public class MapTable<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>
	{

		private struct Entry
		{
			public int hashCode;    // Lower 31 bits of hash code, -1 if unused
			public int next;        // Index of next entry, -1 if last
			public TKey key;           // Key of entry
			public TValue value;         // Value of entry
		}

		private int[] buckets;
		private Entry[] entries;
		private int count;
		private int freeList;
		private int freeCount;
		private readonly IEqualityComparer<TKey> comparer;

		public MapTable() : this(0, null) { }

		public MapTable(int capacity) : this(capacity, null) { }

		public MapTable(IEqualityComparer<TKey> comparer) : this(0, comparer) { }

		public MapTable(int capacity, IEqualityComparer<TKey> comparer)
		{
			if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
			if (capacity > 0) Initialize(capacity);
			this.comparer = comparer ?? EqualityComparer<TKey>.Default;
		}

		public MapTable(IList<KeyValuePair<TKey, TValue>> dictionary, IEqualityComparer<TKey> comparer) :
			this(dictionary != null ? dictionary.Count : 0, comparer)
		{

			if (dictionary == null)
			{
				throw new ArgumentNullException(nameof(dictionary));
			}

			foreach (KeyValuePair<TKey, TValue> pair in dictionary)
			{
				Add(pair.Key, pair.Value);
			}
		}

		public IEqualityComparer<TKey> Comparer => comparer;

		public int Count => count - freeCount;

		public TValue this[TKey key]
		{
			get
			{
				int i = FindEntry(key);
				if (i >= 0)
					return entries[i].value;
				return default(TValue);
			}
			set => Insert(key, value, false);
		}

		public void Add(TKey key, TValue value) => Insert(key, value, true);

		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair) => Add(keyValuePair.Key, keyValuePair.Value);

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
		{
			int i = FindEntry(keyValuePair.Key);
			if (i >= 0 && EqualityComparer<TValue>.Default.Equals(entries[i].value, keyValuePair.Value))
			{
				return true;
			}
			return false;
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
		{
			int i = FindEntry(keyValuePair.Key);
			if (i >= 0 && EqualityComparer<TValue>.Default.Equals(entries[i].value, keyValuePair.Value))
			{
				Remove(keyValuePair.Key);
				return true;
			}
			return false;
		}

		public void Clear()
		{
			if (count > 0)
			{
				for (int i = 0; i < buckets.Length; i++) buckets[i] = -1;
				Array.Clear(entries, 0, count);
				freeList = -1;
				count = 0;
				freeCount = 0;
			}
		}

		public bool ContainsKey(TKey key) => FindEntry(key) >= 0;

		public int IndexOf(TKey key) => FindEntry(key);

		public bool ContainsValue(TValue value)
		{
			if (value == null)
			{
				for (int i = 0; i < count; i++)
				{
					if (entries[i].hashCode >= 0 && entries[i].value == null) return true;
				}
			}
			else
			{
				EqualityComparer<TValue> c = EqualityComparer<TValue>.Default;
				for (int i = 0; i < count; i++)
				{
					if (entries[i].hashCode >= 0 && c.Equals(entries[i].value, value)) return true;
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
				throw new ArgumentOutOfRangeException(nameof(Index));
			}

			if (array.Length - index < Count)
			{
				throw new ArgumentException(nameof(index));
			}

			int count = this.count;
			Entry[] entries = this.entries;
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

			if (buckets != null)
			{
				int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
				for (int i = buckets[hashCode % buckets.Length]; i >= 0; i = entries[i].next)
				{
					if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key)) return i;
				}
			}
			return -1;
		}

		private void Initialize(int capacity)
		{
			int size = capacity;
			buckets = new int[size];
			for (int i = 0; i < buckets.Length; i++) buckets[i] = -1;
			entries = new Entry[size];
			freeList = -1;
		}

		private void Insert(TKey key, TValue value, bool add)
		{

			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			if (buckets == null) Initialize(0);
			int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
			int targetBucket = hashCode % buckets.Length;

			for (int i = buckets[targetBucket]; i >= 0; i = entries[i].next)
			{
				if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key))
				{
					if (add)
					{
						throw new ArgumentException(nameof(key));
					}
					entries[i].value = value;
					return;
				}
			}
			int index;
			if (freeCount > 0)
			{
				index = freeList;
				freeList = entries[index].next;
				freeCount--;
			}
			else
			{
				if (count == entries.Length)
				{
					Resize();
					targetBucket = hashCode % buckets.Length;
				}
				index = count;
				count++;
			}

			entries[index].hashCode = hashCode;
			entries[index].next = buckets[targetBucket];
			entries[index].key = key;
			entries[index].value = value;
			buckets[targetBucket] = index;
		}

		private void Resize()
		{
			Resize(count, false);
		}

		private void Resize(int newSize, bool forceNewHashCodes)
		{
			Contract.Assert(newSize >= entries.Length);
			int[] newBuckets = new int[newSize];
			for (int i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;
			Entry[] newEntries = new Entry[newSize];
			Array.Copy(entries, 0, newEntries, 0, count);
			if (forceNewHashCodes)
			{
				for (int i = 0; i < count; i++)
				{
					if (newEntries[i].hashCode != -1)
					{
						newEntries[i].hashCode = (comparer.GetHashCode(newEntries[i].key) & 0x7FFFFFFF);
					}
				}
			}
			for (int i = 0; i < count; i++)
			{
				if (newEntries[i].hashCode >= 0)
				{
					int bucket = newEntries[i].hashCode % newSize;
					newEntries[i].next = newBuckets[bucket];
					newBuckets[bucket] = i;
				}
			}
			buckets = newBuckets;
			entries = newEntries;
		}

		public bool Remove(TKey key)
		{
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			if (buckets != null)
			{
				int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
				int bucket = hashCode % buckets.Length;
				int last = -1;
				for (int i = buckets[bucket]; i >= 0; last = i, i = entries[i].next)
				{
					if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key))
					{
						if (last < 0)
						{
							buckets[bucket] = entries[i].next;
						}
						else
						{
							entries[last].next = entries[i].next;
						}
						entries[i].hashCode = -1;
						entries[i].next = freeList;
						entries[i].key = default(TKey);
						entries[i].value = default(TValue);
						freeList = i;
						freeCount++;
						return true;
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
				value = entries[i].value;
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
				return entries[i].value;
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
				while ((uint)index < (uint)dictionary.count)
				{
					if (dictionary.entries[index].hashCode >= 0)
					{
						current = new KeyValuePair<TKey, TValue>(dictionary.entries[index].key, dictionary.entries[index].value);
						index++;
						return true;
					}
					index++;
				}

				index = dictionary.count + 1;
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
					if (index == 0 || (index == dictionary.count + 1))
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
