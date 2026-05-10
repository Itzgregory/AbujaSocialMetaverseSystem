namespace AbujaSocialMetaverse.Modules.Core.Public.Models;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    UserDto User
);