namespace Infrastructure.Cache
{
    public interface IRedisCacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<bool> RemoveAsync(string key);
        Task<long> RemoveByPatternAsync(string pattern);
        Task<bool> ExistsAsync(string key);
        Task<bool> ClearAsync();
    }
}
