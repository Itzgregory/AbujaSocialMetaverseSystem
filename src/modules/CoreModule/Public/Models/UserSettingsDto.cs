namespace AbujaSocialMetaverse.Modules.Core.Public.Models;

public record UserSettingsDto(
    bool OpenToNetworking,
    bool OpenToFriends,
    bool OpenToDating,
    int MaxTravelRadiusMeters,
    int MinAgePreference,
    int MaxAgePreference,
    IReadOnlyList<string> Interests
);