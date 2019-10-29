using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace S031.MetaStack.Common
{
	public sealed class ReadOnlyMapTable<TKey, TValue>
	{

		// cache size is always ^2. 
		// items are placed at [hash ^ mask]
		// new item will displace previous one at the same location.
		private readonly int _mask;
		private readonly Entry[] _entries;

		// class, to ensure atomic updates.
		internal class Entry
		{
			internal readonly int hash;
			internal readonly TKey key;
			internal readonly TValue value;

			internal Entry(int hash, TKey key, TValue value)
			{
				this.hash = hash;
				this.key = key;
				this.value = value;
			}
		}

		/// <summary>
		/// Creates a dictionary-like object used for caches.
		/// </summary>
		/// <param name="maxSize">The maximum number of elements to store will be this number aligned to next ^2.</param>
		public ReadOnlyMapTable(int size)
		{
			var alignedSize = AlignSize(size);
			this._mask = alignedSize - 1;
			this._entries = new Entry[alignedSize];
		}

		private static int AlignSize(int size)
		{
			size--;
			size |= size >> 1;
			size |= size >> 2;
			size |= size >> 4;
			size |= size >> 8;
			size |= size >> 16;
			return size + 1;
		}

		/// <summary>
		/// Tries to get the value associated with 'key', returning true if it's found and
		/// false if it's not present.
		/// </summary>
		public bool TryGetValue(TKey key, out TValue value)
		{
			int hash = key.GetHashCode();
			int idx = hash & _mask;

			var entry = Volatile.Read(ref this._entries[idx]);
			if (entry != null && entry.hash == hash && entry.key.Equals(key))
			{
				value = entry.value;
				return true;
			}

			value = default(TValue);
			return false;
		}

		/// <summary>
		/// Adds a new element to the cache, possibly replacing some
		/// element that is already present.
		/// </summary>
		public void Add(TKey key, TValue value)
		{
			var hash = key.GetHashCode();
			var idx = hash & _mask;

			var entry = Volatile.Read(ref this._entries[idx]);
			if (entry == null || entry.hash != hash || !entry.key.Equals(key))
			{
				Volatile.Write(ref _entries[idx], new Entry(hash, key, value));
			}
		}

		/// <summary>
		/// Returns the value associated with the given key, or throws KeyNotFoundException
		/// if the key is not present.
		/// </summary>
		public TValue this[TKey key]
		{
			get
			{
				if (TryGetValue(key, out TValue res))
				{
					return res;
				}
				throw new KeyNotFoundException();
			}
			set
			{
				Add(key, value);
			}
		}
		public int Fragmentation => _entries.Count(e => e == null);
	}
}

