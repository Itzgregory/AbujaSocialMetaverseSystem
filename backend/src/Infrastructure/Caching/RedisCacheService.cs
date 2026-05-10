using MessagePack;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using StackExchange.Redis;

namespace AbujaSocialMetaverse.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly MessagePackSerializerOptions _serializerOptions;
    private readonly AsyncRetryPolicy _retryPolicy;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _db = redis.GetDatabase();
        _logger = logger;

        // LZ4 compression for better performance per optimization docs
        _serializerOptions = MessagePackSerializerOptions.Standard
            .WithCompression(MessagePackCompression.Lz4BlockArray);

        // Retry on transient Redis failures — 3 attempts, exponential backoff
        _retryPolicy = Policy
            .Handle<RedisConnectionException>()
            .Or<RedisTimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                onRetry: (ex, delay, attempt, _) =>
                    _logger.LogWarning(ex,
                        "Redis transient failure. Retry {Attempt} after {Delay}ms",
                        attempt, delay.TotalMilliseconds));
    }

    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await _retryPolicy.ExecuteAsync(
                async () => await _db.StringGetAsync(key));

            if (!data.HasValue) return default;

            return MessagePackSerializer.Deserialize<T>(data!, _serializerOptions);
        }
        catch (RedisException ex)
        {
            throw new CacheException("GET", key, ex.Message, ex);
        }
        catch (MessagePackSerializationException ex)
        {
            _logger.LogError(ex,
                "Deserialization failed for key: {Key}. Deleting stale entry.", key);
            await DeleteAsync(key, cancellationToken);
            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var data = MessagePackSerializer.Serialize(value, _serializerOptions);

            await _retryPolicy.ExecuteAsync(async () =>
            {
                if (expiry.HasValue)
                {
                    await _db.StringSetAsync(key, data, expiry.Value);
                }
                else
                {
                    await _db.StringSetAsync(key, data);
                }
            });
        }
        catch (RedisException ex)
        {
            throw new CacheException("SET", key, ex.Message, ex);
        }
    }

    public async Task<bool> DeleteAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(
                async () => await _db.KeyDeleteAsync(key));
        }
        catch (RedisException ex)
        {
            throw new CacheException("DELETE", key, ex.Message, ex);
        }
    }

    public async Task<bool> ExistsAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(
                async () => await _db.KeyExistsAsync(key));
        }
        catch (RedisException ex)
        {
            throw new CacheException("EXISTS", key, ex.Message, ex);
        }
    }

    public async Task<bool> SetIfNotExistsAsync<T>(
        string key,
        T value,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var data = MessagePackSerializer.Serialize(value, _serializerOptions);
            return await _retryPolicy.ExecuteAsync(
                async () => await _db.StringSetAsync(key, data, expiry, When.NotExists));
        }
        catch (RedisException ex)
        {
            throw new CacheException("SET_NX", key, ex.Message, ex);
        }
    }

    public async Task<long> IncrementAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(
                async () => await _db.StringIncrementAsync(key));
        }
        catch (RedisException ex)
        {
            throw new CacheException("INCR", key, ex.Message, ex);
        }
    }

    public async Task SetExpiryAsync(
        string key,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _retryPolicy.ExecuteAsync(
                async () => await _db.KeyExpireAsync(key, expiry));
        }
        catch (RedisException ex)
        {
            throw new CacheException("EXPIRE", key, ex.Message, ex);
        }
    }

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cached = await GetAsync<T>(key, cancellationToken);
            if (cached is not null) return cached;

            // Acquire a distributed lock to prevent cache stampede
            var lockKey = $"{key}:lock";
            var lockAcquired = await SetIfNotExistsAsync(
                lockKey, true, TimeSpan.FromSeconds(30), cancellationToken);

            if (!lockAcquired)
            {
                // Another instance is refreshing — wait briefly and try cache again
                await Task.Delay(50, cancellationToken);
                var retried = await GetAsync<T>(key, cancellationToken);
                if (retried is not null) return retried;
            }

            try
            {
                var value = await factory(cancellationToken);
                await SetAsync(key, value, expiry, cancellationToken);
                return value;
            }
            finally
            {
                await DeleteAsync(lockKey, cancellationToken);
            }
        }
        catch (CacheException)
        {
            // Cache is unavailable — fall through to factory directly
            _logger.LogWarning(
                "Cache unavailable for key: {Key}. Executing factory directly.", key);
            return await factory(cancellationToken);
        }
    }
}