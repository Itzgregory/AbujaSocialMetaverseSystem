using AbujaSocialMetaverse.Infrastructure.Data;
using AbujaSocialMetaverse.Modules.Core.Public.Interfaces;
using AbujaSocialMetaverse.Shared.Configuration.Options;
using AbujaSocialMetaverse.Shared.Constants;
using AbujaSocialMetaverse.Shared.Helpers;
using AbujaSocialMetaverse.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AbujaSocialMetaverse.Modules.Core.Internal.Services;

public class LockoutService : BaseService, ILockoutService
{
    private readonly LockoutOptions _lockoutOptions;
    private const string UserNotFoundMessage = "User not found.";

    public LockoutService(
        IUnitOfWork unitOfWork,
        IOptions<LockoutOptions> lockoutOptions,
        ILogger<LockoutService> logger)
        : base(logger, unitOfWork)
    {
        _lockoutOptions = lockoutOptions.Value;
    }

    public async Task<Result<bool>> IsLockedOutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(IsLockedOutAsync), async (ct) =>
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));

            var userResult = await GetUserByIdAsync(userId, ct);
            if (!userResult.IsSuccess || userResult.Value is null)
            {
                return Result<bool>.NotFound(ErrorCodes.User.NotFound, UserNotFoundMessage);
            }

            var user = userResult.Value;
            var isLocked = user.LockedUntil.HasValue && user.LockedUntil.Value > DateTimeOffset.UtcNow;

            return Result<bool>.Success(isLocked);
        }, cancellationToken);
    }

    public async Task<Result<int>> GetRemainingLockoutMinutesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(GetRemainingLockoutMinutesAsync), async (ct) =>
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));

            var userResult = await GetUserByIdAsync(userId, ct);
            if (!userResult.IsSuccess || userResult.Value is null)
            {
                return Result<int>.NotFound(ErrorCodes.User.NotFound, UserNotFoundMessage);
            }

            var user = userResult.Value;

            if (!user.LockedUntil.HasValue || user.LockedUntil.Value <= DateTimeOffset.UtcNow)
            {
                return Result<int>.Success(0);
            }

            var remainingMinutes = (int)(user.LockedUntil.Value - DateTimeOffset.UtcNow).TotalMinutes;
            return Result<int>.Success(remainingMinutes);
        }, cancellationToken);
    }

    public async Task<Result> RecordFailedAttemptAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(RecordFailedAttemptAsync), async (ct) =>
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));

            var userResult = await GetUserByIdAsync(userId, ct);
            if (!userResult.IsSuccess || userResult.Value is null)
            {
                return Result.NotFound(ErrorCodes.User.NotFound, UserNotFoundMessage);
            }

            var user = userResult.Value;
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= _lockoutOptions.MaxFailedLoginAttempts)
            {
                user.LockedUntil = DateTimeOffset.UtcNow.AddMinutes(_lockoutOptions.AccountLockoutMinutes);
                _logger.LogWarning(
                    "User {UserId} has been locked out for {Minutes} minutes after {Attempts} failed attempts",
                    userId,
                    _lockoutOptions.AccountLockoutMinutes,
                    user.FailedLoginAttempts);
            }

            return await SaveChangesAsync(ct);
        }, cancellationToken);
    }

    public async Task<Result> ResetLockoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(ResetLockoutAsync), async (ct) =>
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));

            var userResult = await GetUserByIdAsync(userId, ct);
            if (!userResult.IsSuccess || userResult.Value is null)
            {
                return Result.NotFound(ErrorCodes.User.NotFound, UserNotFoundMessage);
            }

            var user = userResult.Value;
            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;

            var saveResult = await SaveChangesAsync(ct);
            if (!saveResult.IsSuccess)
            {
                return saveResult;
            }

            _logger.LogInformation("Lockout reset for user {UserId}", userId);
            return Result.Success();
        }, cancellationToken);
    }
}