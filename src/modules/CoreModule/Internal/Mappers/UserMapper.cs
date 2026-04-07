using AbujaSocialMetaverse.Modules.Core.Public.Models;
using AbujaSocialMetaverse.Shared.Models;

namespace AbujaSocialMetaverse.Modules.Core.Internal.Mappers;

public static class UserMapper
{
    public static UserDto ToDto(
        Guid id,
        string email,
        string displayName,
        string? avatarUrl,
        string? bio,
        SocialMode currentMode,
        DateTimeOffset createdAt,
        bool isActive)
    {
        return new UserDto(id, email, displayName, avatarUrl, bio, currentMode, createdAt, isActive);
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