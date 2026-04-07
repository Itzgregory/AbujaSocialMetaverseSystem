using AbujaSocialMetaverse.Shared.Models;

namespace AbujaSocialMetaverse.Modules.Core.Public;

public interface IUserInterestService
{
    Task<Result<IReadOnlyList<string>>> GetInterestsAsync(CancellationToken cancellationToken = default);
    Task<Result> UpdateInterestsAsync(Guid userId, IReadOnlyList<string> interests, CancellationToken cancellationToken = default);
}