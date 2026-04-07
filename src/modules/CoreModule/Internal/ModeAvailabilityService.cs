using AbujaSocialMetaverse.Infrastructure.Data;
using AbujaSocialMetaverse.Modules.Core.Data.Entities;
using AbujaSocialMetaverse.Modules.Core.Public;
using AbujaSocialMetaverse.Shared.Constants;
using AbujaSocialMetaverse.Shared.Helpers;
using AbujaSocialMetaverse.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AbujaSocialMetaverse.Modules.Core.Internal;

public class ModeAvailabilityService : IModeAvailabilityService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ModeAvailabilityService> _logger;

    public ModeAvailabilityService(ApplicationDbContext context, ILogger<ModeAvailabilityService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> IsModeAvailableAsync(Guid userId, SocialMode mode, CancellationToken cancellationToken = default)
    {
        try
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));
            
            var user = await _context.Set<User>()
                .Include(u => u.DatingProfile)
                .Include(u => u.NetworkingProfile)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
                
            if (user is null)
            {
                return Result<bool>.NotFound(ErrorCodes.User.NotFound, $"User with ID '{userId}' was not found.");
            }
            
            var isAvailable = mode switch
            {
                SocialMode.Dating => user.DatingProfile != null,
                SocialMode.Networking => user.NetworkingProfile != null,
                SocialMode.Leisure => true,
                _ => false
            };
            
            return Result<bool>.Success(isAvailable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check mode availability for user {UserId}, mode {Mode}", userId, mode);
            return Result<bool>.Failure(ErrorCodes.User.ProfileIncomplete, "An error occurred while checking mode availability.");
        }
    }

    public async Task<Result<IReadOnlyList<string>>> GetMissingFieldsForModeAsync(Guid userId, SocialMode mode, CancellationToken cancellationToken = default)
    {
        try
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));
            
            var user = await _context.Set<User>()
                .Include(u => u.DatingProfile)
                .Include(u => u.NetworkingProfile)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
                
            if (user is null)
            {
                return Result<IReadOnlyList<string>>.NotFound(ErrorCodes.User.NotFound, $"User with ID '{userId}' was not found.");
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get missing fields for user {UserId}, mode {Mode}", userId, mode);
            return Result<IReadOnlyList<string>>.Failure(ErrorCodes.User.ProfileIncomplete, "An error occurred while retrieving missing fields.");
        }
    }
}