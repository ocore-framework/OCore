using System;
using System.Threading.Tasks;

namespace OCore.Entities.Data.Extensions
{
    public static class DataCacheExtensions
    {
        public static async Task<DataCache<T>> GetDataCache<T>(this IDataEntity<T> dataSource, TimeSpan? cacheFor)
        {
            var dataCache = new DataCache<T>(dataSource);
            dataCache.CacheFor = cacheFor.Value;
            await dataCache.Refresh();
            return dataCache;
        }
    }
}
