using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Caching
{
	public interface IDataCache<TKey, TValue> : IDictionary<TKey, TValue>
	{
	}
}
