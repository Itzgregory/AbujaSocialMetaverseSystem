using AbujaSocialMetaverse.Infrastructure.Data;
using AbujaSocialMetaverse.Modules.Core.Data.Entities;
using AbujaSocialMetaverse.Modules.Core.Public.Interfaces;
using AbujaSocialMetaverse.Shared.Constants;
using AbujaSocialMetaverse.Shared.Helpers;
using AbujaSocialMetaverse.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AbujaSocialMetaverse.Modules.Core.Internal.Services;

public class UserInterestService : BaseService, IUserInterestService
{
    public UserInterestService(
        IUnitOfWork unitOfWork,
        ILogger<UserInterestService> logger)
        : base(logger, unitOfWork) { }

    public async Task<Result<IReadOnlyList<string>>> GetInterestsAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(GetInterestsAsync), async (ct) =>
        {
            var interests = await _unitOfWork!.Set<Interest>()
                .Where(i => i.IsActive && !i.IsDeleted)
                .OrderBy(i => i.Name)
                .Select(i => i.Name)
                .ToListAsync(ct);

            return Result<IReadOnlyList<string>>.Success(interests.AsReadOnly());
        }, cancellationToken);
    }

    public async Task<Result> UpdateInterestsAsync(
        Guid userId,
        IReadOnlyList<string> interests,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(UpdateInterestsAsync), async (ct) =>
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));
            Guard.Against.Null(interests, nameof(interests));

            var user = await _unitOfWork!.Set<User>()
                .Include(u => u.Interests)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct);

            if (user is null)
            {
                return Result.NotFound(
                    ErrorCodes.User.NotFound,
                    $"User with ID '{userId}' was not found.");
            }

            user.Interests.Clear();

            foreach (var name in interests.Distinct())
            {
                var interest = await _unitOfWork.Set<Interest>()
                    .FirstOrDefaultAsync(i => i.Name == name && !i.IsDeleted, ct);

                if (interest is null)
                {
                    interest = new Interest
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Category = "General",
                        IsActive = true
                    };
                    await _unitOfWork.Set<Interest>().AddAsync(interest, ct);
                }

                user.Interests.Add(new UserInterest
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    InterestId = interest.Id
                });
            }

            return await SaveChangesAsync(ct);
        }, cancellationToken);
    }
}