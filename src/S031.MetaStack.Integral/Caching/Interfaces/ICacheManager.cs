using System;
using System.Collections.Generic;
using System.Text;

namespace S031.MetaStack.Caching
{
	public interface ICacheManager
	{
		IEnumerable<IDataCache<TKey, TValue>> GetCache<TKey, TValue>();
	}
}
