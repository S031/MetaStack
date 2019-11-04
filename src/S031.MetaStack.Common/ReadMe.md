# MetaStack.Common.dll
The **Metastack.Common** library contains a set of common classes used in other Metastack modules.
## Collections
[MapTable<TKey, TValue>](https://github.com/S031/MetaStack/blob/master/src/S031.MetaStack.Common/Collections/MapTable.cs) Minimalistic thread safe [dictionary](https://en.wikipedia.org/wiki/Hash_table) implementatition with minimal memory fragmentation (original algorithm collisions control) 
MapTable is based on Dictionary code from Microsoft, but unlike Dictionary bucket array has a size different from the size of entries implemented in the Resize method:

```csharp
private void Resize()
{
	//New entries size
	int newSize = _count * 2;
	double border = Math.Log2(newSize);
	int delta = Convert.ToInt32(Math.Pow(2, _collisionCount / border));
	//New buckets size depended on new entries size, 
	//collisionCount and evaluated collision border size
	int bSize = _buckets.Length * delta;

	int[] newBuckets = new int[bSize];
	Array.Fill(newBuckets, -1);

	Entry[] newEntries = new Entry[newSize];
	Array.Copy(_entries, 0, newEntries, 0, _count);

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
	
	//For test only
	if (_collisionCount > Collisions)
		Collisions = _collisionCount;
	_collisionCount = 0;
}
```
Original [Dictionary](https://github.com/microsoft/referencesource/blob/master/mscorlib/system/collections/generic/dictionary.cs) Has a fragmentation of over 50%

```csharp
public int Fragmentation => this.buckets.Count(i => i == -1) - _freeCount;
```
**MapTable** has a fragmentation less then [0.3%](https://github.com/S031/MetaStack/blob/5dccc5438580ac5218e3c4e0639f31adef365e99/Test/MetaStack.Test/Common/MapTableTest.cs#L19) . without a performance degradation

**[ReadOnlyCache<TKey, TValue>]**(https://github.com/S031/MetaStack/blob/master/src/S031.MetaStack.Common/Collections/ReadOnlyCache.cs) class for stor readonly array of data with binary search by key hash code smaller then readonly dictionary and no memory fragmentation. Useful for Switch/Case statement replacement or caching static data. Used [Array.Sort](https://docs.microsoft.com/en-us/dotnet/api/system.array.sort?view=netframework-4.8#System_Array_Sort_System_Array_System_Array_) and [Array.BinarySearch](https://docs.microsoft.com/en-us/dotnet/api/system.array.binarysearch?view=netcore-3.0) for creating and fastest search of keys.
