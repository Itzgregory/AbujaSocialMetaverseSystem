using AbujaSocialMetaverse.Shared.Models;

namespace AbujaSocialMetaverse.Modules.Core.Public;

public interface IModeAvailabilityService
{
    Task<Result<bool>> IsModeAvailableAsync(Guid userId, SocialMode mode, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<string>>> GetMissingFieldsForModeAsync(Guid userId, SocialMode mode, CancellationToken cancellationToken = default);
}