using Microsoft.AspNetCore.SignalR;

namespace AbujaSocialMetaverse.Infrastructure.RealTime;

/// <summary>
/// Marker class for AvatarHub.
/// Actual hub implementation lives in the API layer (Phase 5).
/// Infrastructure references this marker so IRealTimeService
/// can be wired without depending on the API project.
/// </summary>
public abstract class AvatarHubMarker : Hub { }

/// <summary>
/// Marker class for ChatHub.
/// </summary>
public abstract class ChatHubMarker : Hub { }