using System.Text.Json;
using StackExchange.Redis;

namespace CodeRunner.Executor.Services;

public interface ICacheService
{
    Task<(bool success, TValue? value)> TryGetValue<TKey, TValue>(TKey key);
    Task<bool> SetValue<TKey, TValue>(TKey key, TValue value, TimeSpan? expiry = null, When when  = When.Always);
}

public class CacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;

    public CacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<(bool success, TValue? value)> TryGetValue<TKey, TValue>(TKey key)
    {
        var db = _redis.GetDatabase();

        var keyJson = JsonSerializer.Serialize(key);
        var redisValue = await db.StringGetAsync(keyJson);

        TValue? value = default;
        bool success = false;

        if (redisValue.HasValue)
        {
           value = JsonSerializer.Deserialize<TValue>(redisValue.ToString());
           success = true;
        }

        return (success, value);
    }

    public async Task<bool> SetValue<TKey, TValue>(TKey key, TValue value, TimeSpan? expiry, When when)
    {
        var db = _redis.GetDatabase();

        var keyJson = JsonSerializer.Serialize(key);
        var valueJson = JsonSerializer.Serialize(value);
        var cached = await db.StringSetAsync(keyJson, valueJson, expiry, when);

        return cached;
    }
}