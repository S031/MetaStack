using S031.MetaStack.Caching;
using S031.MetaStack.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace S031.MetaStack.Caching
{
	/// <summary>
	/// Using:
	///		Services.GetRequiredService<ICacheManger>
	///			.GetCache<string, UserInfo>()
	///			.Remove("username1")
	///			.Remove("username2");
	/// </summary>
	public class CacheManager : ICacheManager
	{
		public IEnumerable<IDataCache<TKey, TValue>> GetCache<TKey, TValue>()
			=> ImplementsList.GetTypes(typeof(IDataCache<TKey, TValue>))
				.Select(t => t.CreateInstance<IDataCache<TKey, TValue>>());
	}

	public static class IDataCachingExtension
	{
		public static IEnumerable<IDataCache<TKey, TValue>> RemnoveAll<TKey, TValue>(this IEnumerable<IDataCache<TKey, TValue>> caches)
		{
			foreach (var cache in caches)
				cache?.Clear();
			return caches;
		}
		public static async Task<IEnumerable<IDataCache<TKey, TValue>>> RemnoveAllAsinc<TKey, TValue>(this IEnumerable<IDataCache<TKey, TValue>> caches)
			=> await Task.Run(() => caches.RemnoveAll());

		public static IEnumerable<IDataCache<TKey, TValue>> Remove<TKey, TValue>(this IEnumerable<IDataCache<TKey, TValue>> caches, TKey key)
		{
			foreach (var cache in caches)
				cache?.Remove(key);
			return caches;
		}
		public static async Task RemoveAsync<TKey, TValue>(this IEnumerable<IDataCache<TKey, TValue>> caches, TKey key)
			=> await Task.Run(() => caches.Remove(key));
	}
}
