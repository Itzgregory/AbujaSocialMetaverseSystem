using AbujaSocialMetaverse.Infrastructure.RealTime.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace AbujaSocialMetaverse.Infrastructure.RealTime;

public class SignalRRealTimeService : IRealTimeService
{
    private readonly IHubContext<AvatarHubMarker> _avatarHub;
    private readonly IHubContext<ChatHubMarker> _chatHub;
    private readonly IConnectionTracker _connectionTracker;
    private readonly ILogger<SignalRRealTimeService> _logger;

    public SignalRRealTimeService(
        IHubContext<AvatarHubMarker> avatarHub,
        IHubContext<ChatHubMarker> chatHub,
        IConnectionTracker connectionTracker,
        ILogger<SignalRRealTimeService> logger)
    {
        _avatarHub = avatarHub;
        _chatHub = chatHub;
        _connectionTracker = connectionTracker;
        _logger = logger;
    }

    public async Task SendPositionUpdateAsync(
        string regionGroup,
        AvatarPositionUpdate update,
        CancellationToken cancellationToken = default)
    {
        await _avatarHub.Clients
            .Group(regionGroup)
            .SendAsync("PositionUpdate", update, cancellationToken);
    }

    public async Task JoinRegionAsync(
        string connectionId,
        string regionGroup,
        CancellationToken cancellationToken = default)
    {
        await _avatarHub.Groups
            .AddToGroupAsync(connectionId, regionGroup, cancellationToken);

        _logger.LogDebug(
            "Connection {ConnectionId} joined region group {RegionGroup}",
            connectionId, regionGroup);
    }

    public async Task LeaveRegionAsync(
        string connectionId,
        string regionGroup,
        CancellationToken cancellationToken = default)
    {
        await _avatarHub.Groups
            .RemoveFromGroupAsync(connectionId, regionGroup, cancellationToken);

        _logger.LogDebug(
            "Connection {ConnectionId} left region group {RegionGroup}",
            connectionId, regionGroup);
    }

    public async Task SendMatchNotificationAsync(
        Guid userAId,
        Guid userBId,
        MatchNotification notification,
        CancellationToken cancellationToken = default)
    {
        var tasks = new[]
        {
            SendToUserAsync(userAId, "MatchFound", notification, cancellationToken),
            SendToUserAsync(userBId, "MatchFound", notification, cancellationToken)
        };

        await Task.WhenAll(tasks);

        _logger.LogInformation(
            "Match notification sent to users {UserAId} and {UserBId}",
            userAId, userBId);
    }

    public async Task OpenChatSessionAsync(
        Guid userAId,
        Guid userBId,
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        var connectionAId = await _connectionTracker.GetConnectionIdAsync(
            userAId, cancellationToken);
        var connectionBId = await _connectionTracker.GetConnectionIdAsync(
            userBId, cancellationToken);

        var tasks = new List<Task>();

        if (connectionAId is not null)
            tasks.Add(_chatHub.Groups.AddToGroupAsync(
                connectionAId, sessionId, cancellationToken));

        if (connectionBId is not null)
            tasks.Add(_chatHub.Groups.AddToGroupAsync(
                connectionBId, sessionId, cancellationToken));

        await Task.WhenAll(tasks);

        _logger.LogInformation(
            "Chat session {SessionId} opened for users {UserAId} and {UserBId}",
            sessionId, userAId, userBId);
    }

    public async Task SendChatMessageAsync(
        string sessionId,
        ChatMessage message,
        CancellationToken cancellationToken = default)
    {
        await _chatHub.Clients
            .Group(sessionId)
            .SendAsync("ReceiveMessage", message, cancellationToken);
    }

    public async Task LeaveChatSessionAsync(
        string connectionId,
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        await _chatHub.Groups
            .RemoveFromGroupAsync(connectionId, sessionId, cancellationToken);

        _logger.LogDebug(
            "Connection {ConnectionId} left chat session {SessionId}",
            connectionId, sessionId);
    }

    public async Task SendToUserAsync<T>(
        Guid userId,
        string eventName,
        T payload,
        CancellationToken cancellationToken = default)
    {
        var connectionId = await _connectionTracker
            .GetConnectionIdAsync(userId, cancellationToken);

        if (connectionId is null)
        {
            _logger.LogDebug(
                "Cannot send event {EventName} to user {UserId} — not connected",
                eventName, userId);
            return;
        }

        await _avatarHub.Clients
            .Client(connectionId)
            .SendAsync(eventName, payload, cancellationToken);
    }
}