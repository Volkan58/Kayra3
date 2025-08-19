using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Cache
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IConnectionMultiplexer redis, IDatabase database, ILogger<RedisCacheService> logger)
        {
            _redis = redis;
            _database = database;
            _logger = logger;
        }

        public async Task<bool> ClearAsync()
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                await server.FlushDatabaseAsync();

                _logger.LogInformation("Cache başarıyla temizlendi");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache temizlenirken hata oluştu");
                return false;
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _database.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Key varlığı kontrol edilirken hata oluştu: {Key}", key);
                return false;
            }
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await _database.StringGetAsync(key);
                if (!value.HasValue)
                {
                    return default;
                }

                return JsonSerializer.Deserialize<T>(value!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache'den veri alınırken hata oluştu: {Key}", key);
                return default;
            }
        }

        public async Task<bool> RemoveAsync(string key)
        {
            try
            {
                return await _database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache'den veri silinirken hata oluştu: {Key}", key);
                return false;
            }
        }

        public async Task<long> RemoveByPatternAsync(string pattern)
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var keys = server.Keys(pattern: pattern);
                var deletedCount = 0L;

                foreach (var key in keys)
                {
                    if (await _database.KeyDeleteAsync(key))
                    {
                        deletedCount++;
                    }
                }

                _logger.LogInformation("Pattern '{Pattern}' ile {Count} key silindi", pattern, deletedCount);
                return deletedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pattern ile key silinirken hata oluştu: {Pattern}", pattern);
                return 0;
            }
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var serializedValue = JsonSerializer.Serialize(value);
                return await _database.StringSetAsync(key, serializedValue, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache'e veri kaydedilirken hata oluştu: {Key}", key);
                return false;
            }
        }
    }
}
