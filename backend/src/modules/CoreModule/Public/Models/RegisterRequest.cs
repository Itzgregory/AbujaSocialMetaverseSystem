namespace AbujaSocialMetaverse.Modules.Core.Public.Models;

public record RegisterRequest(
    string Email,
    string Password,
    string DisplayName
);