using MessagePack;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using StackExchange.Redis;

namespace AbujaSocialMetaverse.Infrastructure.Caching;

public class RedisLocationCacheService : ILocationCacheService
{
    private readonly IDatabase _db;
    private readonly ILogger<RedisLocationCacheService> _logger;
    private readonly MessagePackSerializerOptions _serializerOptions;
    private readonly AsyncRetryPolicy _retryPolicy;

    public RedisLocationCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisLocationCacheService> logger)
    {
        _db = redis.GetDatabase();
        _logger = logger;

        _serializerOptions = MessagePackSerializerOptions.Standard
            .WithCompression(MessagePackCompression.Lz4BlockArray);

        _retryPolicy = Policy
            .Handle<RedisConnectionException>()
            .Or<RedisTimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                onRetry: (ex, delay, attempt, _) =>
                    _logger.LogWarning(ex,
                        "Redis transient failure on location cache. Retry {Attempt}",
                        attempt));
    }

    public async Task HashSetAsync<T>(
        string key,
        string field,
        T value,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var data = MessagePackSerializer.Serialize(value, _serializerOptions);
            await _retryPolicy.ExecuteAsync(
                async () => await _db.HashSetAsync(key, field, data));
        }
        catch (RedisException ex)
        {
            throw new CacheException("HSET", $"{key}:{field}", ex.Message, ex);
        }
    }

    public async Task<T?> HashGetAsync<T>(
        string key,
        string field,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await _retryPolicy.ExecuteAsync(
                async () => await _db.HashGetAsync(key, field));

            if (!data.HasValue) return default;
            return MessagePackSerializer.Deserialize<T>(data!, _serializerOptions);
        }
        catch (RedisException ex)
        {
            throw new CacheException("HGET", $"{key}:{field}", ex.Message, ex);
        }
    }

    public async Task<bool> HashDeleteAsync(
        string key,
        string field,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(
                async () => await _db.HashDeleteAsync(key, field));
        }
        catch (RedisException ex)
        {
            throw new CacheException("HDEL", $"{key}:{field}", ex.Message, ex);
        }
    }

    public async Task<IEnumerable<T>> HashGetAllAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entries = await _retryPolicy.ExecuteAsync(
                async () => await _db.HashGetAllAsync(key));

            var results = new List<T>();
            foreach (var entry in entries)
            {
                try
                {
                    var item = MessagePackSerializer.Deserialize<T>(
                        entry.Value!, _serializerOptions);
                    results.Add(item);
                }
                catch (MessagePackSerializationException ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to deserialize hash entry {Field} in key {Key}. Skipping.",
                        entry.Name, key);
                }
            }
            return results;
        }
        catch (RedisException ex)
        {
            throw new CacheException("HGETALL", key, ex.Message, ex);
        }
    }

    public async Task SetAddAsync(
        string key,
        string value,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _retryPolicy.ExecuteAsync(
                async () => await _db.SetAddAsync(key, value));

            if (expiry.HasValue)
                await _db.KeyExpireAsync(key, expiry);
        }
        catch (RedisException ex)
        {
            throw new CacheException("SADD", key, ex.Message, ex);
        }
    }

    public async Task SetRemoveAsync(
        string key,
        string value,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _retryPolicy.ExecuteAsync(
                async () => await _db.SetRemoveAsync(key, value));
        }
        catch (RedisException ex)
        {
            throw new CacheException("SREM", key, ex.Message, ex);
        }
    }

    public async Task<IEnumerable<string>> SetMembersAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var members = await _retryPolicy.ExecuteAsync(
                async () => await _db.SetMembersAsync(key));
            return members.Select(m => m.ToString()).ToList();
        }
        catch (RedisException ex)
        {
            throw new CacheException("SMEMBERS", key, ex.Message, ex);
        }
    }

    public async Task<bool> SetContainsAsync(
        string key,
        string value,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(
                async () => await _db.SetContainsAsync(key, value));
        }
        catch (RedisException ex)
        {
            throw new CacheException("SISMEMBER", key, ex.Message, ex);
        }
    }
}