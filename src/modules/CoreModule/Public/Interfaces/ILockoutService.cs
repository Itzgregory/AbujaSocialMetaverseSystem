using AbujaSocialMetaverse.Shared.Models;

namespace AbujaSocialMetaverse.Modules.Core.Public.Interfaces;

public interface ILockoutService
{
    Task<Result<bool>> IsLockedOutAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<int>> GetRemainingLockoutMinutesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result> RecordFailedAttemptAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result> ResetLockoutAsync(Guid userId, CancellationToken cancellationToken = default);
}