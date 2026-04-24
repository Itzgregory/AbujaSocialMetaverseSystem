using AbujaSocialMetaverse.Modules.Core.Data.Entities;
using AbujaSocialMetaverse.Modules.Core.Public.Models;
using AbujaSocialMetaverse.Shared.Models;

namespace AbujaSocialMetaverse.Modules.Core.Internal.Mappers;

public static class UserMapper
{
    public static UserDto ToDto(User user)
    {
        return new UserDto(
            user.Id,
            user.Email,
            user.DisplayName,
            user.AvatarUrl,
            user.Bio,
            user.CurrentMode,
            user.CreatedAt,
            user.IsActive,
            user.EmailVerified
        );
    }

    public static UserSettingsDto ToSettingsDto(
        bool openToNetworking,
        bool openToFriends,
        bool openToDating,
        int maxTravelRadiusMeters,
        int minAgePreference,
        int maxAgePreference,
        IReadOnlyList<string> interests)
    {
        return new UserSettingsDto(
            openToNetworking,
            openToFriends,
            openToDating,
            maxTravelRadiusMeters,
            minAgePreference,
            maxAgePreference,
            interests);
    }
}