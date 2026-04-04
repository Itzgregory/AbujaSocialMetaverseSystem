using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AbujaSocialMetaverse.Infrastructure.Caching;

public class RedisCacheAdminService : ICacheAdminService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly ILogger<RedisCacheAdminService> _logger;

    public RedisCacheAdminService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheAdminService> logger)
    {
        _redis = redis;
        _db = redis.GetDatabase();
        _logger = logger;
    }

    public async Task DeleteByPatternAsync(
        string pattern,
        int batchSize = 100,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "Cache admin: DeleteByPattern called with pattern {Pattern}", pattern);

        var deletedCount = 0;

        foreach (var endpoint in _redis.GetEndPoints())
        {
            var server = _redis.GetServer(endpoint);
            if (server.IsReplica) continue;

            var batch = new List<RedisKey>(batchSize);

            await foreach (var key in server.KeysAsync(pattern: pattern))
            {
                cancellationToken.ThrowIfCancellationRequested();
                batch.Add(key);

                if (batch.Count >= batchSize)
                {
                    await _db.KeyDeleteAsync(batch.ToArray());
                    deletedCount += batch.Count;
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
            {
                await _db.KeyDeleteAsync(batch.ToArray());
                deletedCount += batch.Count;
            }
        }

        _logger.LogInformation(
            "Cache admin: Deleted {Count} keys matching pattern {Pattern}",
            deletedCount, pattern);
    }

    public async Task<long> CountByPatternAsync(
        string pattern,
        CancellationToken cancellationToken = default)
    {
        long count = 0;

        foreach (var endpoint in _redis.GetEndPoints())
        {
            var server = _redis.GetServer(endpoint);
            if (server.IsReplica) continue;

            await foreach (var _ in server.KeysAsync(pattern: pattern))
            {
                cancellationToken.ThrowIfCancellationRequested();
                count++;
            }
        }

        return count;
    }

    public async Task FlushUserDataAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Cache admin: Flushing all cache data for user {UserId}", userId);

        var patterns = new[]
        {
            CacheKeys.Users.Session(userId),
            CacheKeys.Users.Profile(userId),
            CacheKeys.Users.Consent(userId),
            CacheKeys.Users.RefreshToken(userId),
            CacheKeys.Avatars.Location(userId),
            CacheKeys.Recommendations.ForUser(userId, "*")
        };

        foreach (var pattern in patterns)
        {
            await DeleteByPatternAsync(pattern, cancellationToken: cancellationToken);
        }
    }
}