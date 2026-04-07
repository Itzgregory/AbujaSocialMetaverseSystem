using AbujaSocialMetaverse.Shared.Models;

namespace AbujaSocialMetaverse.Modules.Core.Public.Models;

public record UpdateSettingsRequest(
    SocialMode CurrentMode,
    bool OpenToNetworking,
    bool OpenToFriends,
    bool OpenToDating,
    int? MaxTravelRadiusMeters,
    int? MinAgePreference,
    int? MaxAgePreference,
    IReadOnlyList<string>? Interests
);