namespace AbujaSocialMetaverse.Modules.Core.Public.Models;

public record UpdateProfileRequest(
    string? DisplayName,
    string? Bio,
    string? AvatarUrl
);