using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace CodeRunner.Executor.Services;

public interface ICacheService
{
    Task<(bool success, TValue? value)> TryGetValue<TKey, TValue>(TKey key);
    Task SetValue<TKey, TValue>(TKey key, TValue value);
}

public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;

    public CacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<(bool success, TValue? value)> TryGetValue<TKey, TValue>(TKey key)
    {
        var keyJson = JsonSerializer.Serialize(key);
        var cachedValue = await _distributedCache.GetStringAsync(keyJson);

        TValue? value = default;
        bool success = false;

        if (!string.IsNullOrEmpty(cachedValue))
        {
           value = JsonSerializer.Deserialize<TValue>(cachedValue);
           success = true;
        }

        return (success, value);
    }

    public async Task SetValue<TKey, TValue>(TKey key, TValue value)
    {
        var keyJson = JsonSerializer.Serialize(key);
        var valueJson = JsonSerializer.Serialize(value);

        await _distributedCache.SetStringAsync(keyJson, valueJson);
    }
}