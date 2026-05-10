namespace AbujaSocialMetaverse.Modules.Core.Public.Models;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);