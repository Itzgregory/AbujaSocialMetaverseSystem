namespace AbujaSocialMetaverse.Shared.Models;

/// <summary>
/// Self-contained DTO assembled at the controller layer
/// before being passed to the CompatibilityEngine in SocialModule.
/// The engine never imports Core or Business interfaces directly.
/// </summary>
public record CompatibilityContext
{
    public Guid UserAId { get; init; }
    public Guid UserBId { get; init; }
    public SocialMode UserAMode { get; init; }
    public SocialMode UserBMode { get; init; }
    public IReadOnlyList<string> UserAInterests { get; init; } = [];
    public IReadOnlyList<string> UserBInterests { get; init; } = [];
    public bool UserAOpenToNetworking { get; init; }
    public bool UserBOpenToNetworking { get; init; }
    public bool UserAOpenToFriends { get; init; }
    public bool UserBOpenToFriends { get; init; }
    public bool UserAOpenToDating { get; init; }
    public bool UserBOpenToDating { get; init; }
    public float DistanceMeters { get; init; }

    public void Validate()
    {
        if (UserAId == Guid.Empty)
            throw new ArgumentException("UserAId cannot be empty.", nameof(UserAId));

        if (UserBId == Guid.Empty)
            throw new ArgumentException("UserBId cannot be empty.", nameof(UserBId));

        if (UserAId == UserBId)
            throw new ArgumentException("UserAId and UserBId cannot be the same user.");

        if (DistanceMeters < 0)
            throw new ArgumentException("DistanceMeters cannot be negative.",
                nameof(DistanceMeters));
    }
}

public enum SocialMode
{
    None = 0,
    Dating = 1,
    Networking = 2,
    Leisure = 3
}