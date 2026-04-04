using AbujaSocialMetaverse.Infrastructure.Caching;
using Microsoft.Extensions.Logging;

namespace AbujaSocialMetaverse.Infrastructure.RealTime;

public class RedisConnectionTracker : IConnectionTracker
{
    private readonly ICacheService _cache;
    private readonly ILogger<RedisConnectionTracker> _logger;
    private static readonly TimeSpan ConnectionExpiry = TimeSpan.FromHours(24);

    public RedisConnectionTracker(
        ICacheService cache,
        ILogger<RedisConnectionTracker> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task RegisterAsync(
        Guid userId,
        string connectionId,
        CancellationToken cancellationToken = default)
    {
        var key = ConnectionKey(userId);
        await _cache.SetAsync(key, connectionId, ConnectionExpiry, cancellationToken);
        _logger.LogDebug("Registered connection {ConnectionId} for user {UserId}",
            connectionId, userId);
    }

    public async Task UnregisterAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var key = ConnectionKey(userId);
        await _cache.DeleteAsync(key, cancellationToken);
        _logger.LogDebug("Unregistered connection for user {UserId}", userId);
    }

    public async Task<string?> GetConnectionIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var key = ConnectionKey(userId);
        return await _cache.GetAsync<string>(key, cancellationToken);
    }

    public async Task<bool> IsOnlineAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var key = ConnectionKey(userId);
        return await _cache.ExistsAsync(key, cancellationToken);
    }

    private static string ConnectionKey(Guid userId) =>
        $"asm:connection:{userId}";
}