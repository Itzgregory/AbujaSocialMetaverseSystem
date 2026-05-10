namespace AbujaSocialMetaverse.Modules.Core.Public.Models;
using AbujaSocialMetaverse.Shared.Models;

public record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    string? Bio,
    SocialMode CurrentMode,
    DateTimeOffset CreatedAt,
    bool IsActive,
    bool EmailVerified = false
);