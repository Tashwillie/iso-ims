using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mindflur.IMS.Application.Contracts.Service;
using Mindflur.IMS.Application.Core;
using Mindflur.IMS.Data.Models;

namespace Mindflur.IMS.Business.Service
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly MemoryCacheEntryOptions _cacheOptions;
        private readonly ILogger _logger;
        private readonly bool _disableCache;

        public MemoryCacheService(IMemoryCache memoryCache, IOptions<CoreSettings> coreConfig, ILogger<MemoryCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            var _cacheConfig = coreConfig.Value;

            int slidingExpiration = 60*_cacheConfig.CacheConfiguration.SlidingExpirationInMinutes;
            int absoluteExpiration = 60*60*_cacheConfig.CacheConfiguration.AbsoluteExpirationInHours;
            long size = _cacheConfig.CacheConfiguration.Size;
            _disableCache = coreConfig.Value.CacheConfiguration.DisableCache;

            if (_cacheConfig == null)
            {
                slidingExpiration = 60;
                absoluteExpiration = 3660;
                _logger.LogWarning($"Please setup 'CacheConfiguration'. Default slidingExpiration '{slidingExpiration}' and absoluteExpiration '{absoluteExpiration}' and size {size}");
                _disableCache = true;
            }

            

            _cacheOptions = new MemoryCacheEntryOptions()
               .SetSlidingExpiration(TimeSpan.FromSeconds(slidingExpiration))
               .SetAbsoluteExpiration(TimeSpan.FromSeconds(absoluteExpiration))
               .SetPriority(CacheItemPriority.Normal)
               .SetSize(size);
        }

        public bool TryGet<T>(string cacheKey, out T value)
        {
            _logger.LogInformation($"Getting cache key: {cacheKey}");
            _memoryCache.TryGetValue(cacheKey, out value);
            return value != null;
        }

        public T Set<T>(string cacheKey, T value)
        {
            _logger.LogInformation($"Setting cache key: {cacheKey}");

            if (_disableCache)
            {
                cacheKey = string.Empty;
            }

            return _memoryCache.Set(cacheKey, value, _cacheOptions);
        }

        public void Remove(string cacheKey)
        {
            _logger.LogInformation($"Remove cache key: {cacheKey}");

            _memoryCache.Remove(cacheKey);
        }
    }
}