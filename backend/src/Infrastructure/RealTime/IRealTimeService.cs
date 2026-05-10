using AbujaSocialMetaverse.Infrastructure.RealTime.Models;

namespace AbujaSocialMetaverse.Infrastructure.RealTime;

/// <summary>
/// Abstraction over the real-time transport layer.
/// Current implementation: SignalR with Redis backplane.
/// Swappable without touching any module — just register a different implementation in DI.
/// </summary>
public interface IRealTimeService
{
    // ─── Avatar ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Broadcast a position update to all subscribers in a region group.
    /// </summary>
    Task SendPositionUpdateAsync(
        string regionGroup,
        AvatarPositionUpdate update,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a connection to a region group so it receives position broadcasts.
    /// </summary>
    Task JoinRegionAsync(
        string connectionId,
        string regionGroup,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a connection from a region group.
    /// </summary>
    Task LeaveRegionAsync(
        string connectionId,
        string regionGroup,
        CancellationToken cancellationToken = default);

    // ─── Proximity & Matching ─────────────────────────────────────────────────

    /// <summary>
    /// Notify both users that they are within proximity of each other.
    /// Compatibility check is handled by the backend before calling this.
    /// </summary>
    Task SendMatchNotificationAsync(
        Guid userAId,
        Guid userBId,
        MatchNotification notification,
        CancellationToken cancellationToken = default);

    // ─── Chat ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Open a chat session between two users by adding both
    /// connections to a shared session group.
    /// </summary>
    Task OpenChatSessionAsync(
        Guid userAId,
        Guid userBId,
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a chat message to all members of a session group.
    /// </summary>
    Task SendChatMessageAsync(
        string sessionId,
        ChatMessage message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a user's connection from a chat session group.
    /// </summary>
    Task LeaveChatSessionAsync(
        string connectionId,
        string sessionId,
        CancellationToken cancellationToken = default);

    // ─── Connection Management ────────────────────────────────────────────────

    /// <summary>
    /// Send a message directly to a specific user by their userId.
    /// Uses the connection tracker to resolve connectionId.
    /// </summary>
    Task SendToUserAsync<T>(
        Guid userId,
        string eventName,
        T payload,
        CancellationToken cancellationToken = default);
}