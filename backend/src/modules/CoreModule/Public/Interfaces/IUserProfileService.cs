using AbujaSocialMetaverse.Modules.Core.Public.Models;
using AbujaSocialMetaverse.Shared.Models;

namespace AbujaSocialMetaverse.Modules.Core.Public.Interfaces;

public interface IUserProfileService
{
    Task<Result<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<Result<UserSettingsDto>> UpdateSettingsAsync(Guid userId, UpdateSettingsRequest request, CancellationToken cancellationToken = default);
    Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the user's password hash directly (used for hash upgrades)
    /// </summary>
    Task<Result> UpdatePasswordHashAsync(Guid userId, string newPasswordHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the user's last login information (timestamp and IP address)
    /// </summary>
    Task<Result> UpdateLastLoginInfoAsync(Guid userId, string ipAddress, CancellationToken cancellationToken = default);
}