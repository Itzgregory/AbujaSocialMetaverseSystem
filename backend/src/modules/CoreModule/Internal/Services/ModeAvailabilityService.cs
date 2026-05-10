using AbujaSocialMetaverse.Infrastructure.Data;
using AbujaSocialMetaverse.Modules.Core.Data.Entities;
using AbujaSocialMetaverse.Modules.Core.Public.Interfaces;
using AbujaSocialMetaverse.Shared.Constants;
using AbujaSocialMetaverse.Shared.Helpers;
using AbujaSocialMetaverse.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AbujaSocialMetaverse.Modules.Core.Internal.Services;

public class ModeAvailabilityService : BaseService, IModeAvailabilityService
{
    public ModeAvailabilityService(
        IUnitOfWork unitOfWork,
        ILogger<ModeAvailabilityService> logger)
        : base(logger, unitOfWork)
    {
    }

    public async Task<Result<bool>> IsModeAvailableAsync(
        Guid userId,
        SocialMode mode,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(IsModeAvailableAsync), async (ct) =>
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));
            
            var user = await _unitOfWork!.Set<User>()
                .Include(u => u.DatingProfile)
                .Include(u => u.NetworkingProfile)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct);
                
            if (user is null)
            {
                return Result<bool>.NotFound(
                    ErrorCodes.User.NotFound,
                    $"User with ID '{userId}' was not found.");
            }
            
            bool isAvailable = mode switch
            {
                SocialMode.Dating => user.DatingProfile != null,
                SocialMode.Networking => user.NetworkingProfile != null,
                SocialMode.Leisure => true,
                _ => false
            };
            
            return Result<bool>.Success(isAvailable);
        }, cancellationToken);
    }

    public async Task<Result<IReadOnlyList<string>>> GetMissingFieldsForModeAsync(
        Guid userId,
        SocialMode mode,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(GetMissingFieldsForModeAsync), async (ct) =>
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));
            
            var user = await _unitOfWork!.Set<User>()
                .Include(u => u.DatingProfile)
                .Include(u => u.NetworkingProfile)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct);
                
            if (user is null)
            {
                return Result<IReadOnlyList<string>>.NotFound(
                    ErrorCodes.User.NotFound,
                    $"User with ID '{userId}' was not found.");
            }
            
            var missingFields = new List<string>();
            
            if (mode == SocialMode.Dating && user.DatingProfile is null)
            {
                missingFields.Add("Dating Profile (Date of Birth, Gender, Gender Preference)");
            }
            else if (mode == SocialMode.Networking && user.NetworkingProfile is null)
            {
                missingFields.Add("Networking Profile (Industry, Occupation)");
            }
            
            return Result<IReadOnlyList<string>>.Success(missingFields.AsReadOnly());
        }, cancellationToken);
    }
}