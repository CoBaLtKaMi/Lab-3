using StackExchange.Redis;
using System.Text.Json;

namespace SportClubApi.Services;

public class CacheService
{
    private readonly IDatabase _db;

    public CacheService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    // Получить данные из кэша. Возвращает null если кэш пуст.
    public async Task<T?> GetAsync<T>(string key)
    {
        var val = await _db.StringGetAsync(key);
        if (val.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>(val!);
    }

    // Сохранить данные в кэш с временем жизни TTL
    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, ttl);
    }

    // Удалить запись из кэша (при изменении данных)
    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }
}
