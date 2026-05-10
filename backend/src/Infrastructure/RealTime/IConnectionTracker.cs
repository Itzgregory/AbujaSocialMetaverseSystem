namespace AbujaSocialMetaverse.Infrastructure.RealTime;

/// <summary>
/// Tracks the mapping between userId and SignalR connectionId.
/// Stored in Redis so it works across multiple monolith instances.
/// </summary>
public interface IConnectionTracker
{
    Task RegisterAsync(Guid userId, string connectionId, CancellationToken cancellationToken = default);
    Task UnregisterAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<string?> GetConnectionIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsOnlineAsync(Guid userId, CancellationToken cancellationToken = default);
}