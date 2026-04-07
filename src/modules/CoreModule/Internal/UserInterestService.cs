using AbujaSocialMetaverse.Infrastructure.Data;
using AbujaSocialMetaverse.Modules.Core.Data.Entities;
using AbujaSocialMetaverse.Modules.Core.Public;
using AbujaSocialMetaverse.Shared.Constants;
using AbujaSocialMetaverse.Shared.Helpers;
using AbujaSocialMetaverse.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AbujaSocialMetaverse.Modules.Core.Internal;

public class UserInterestService : IUserInterestService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserInterestService> _logger;

    public UserInterestService(ApplicationDbContext context, ILogger<UserInterestService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<string>>> GetInterestsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var interests = await _context.Set<Interest>()
                .Where(i => i.IsActive && !i.IsDeleted)
                .OrderBy(i => i.Name)
                .Select(i => i.Name)
                .ToListAsync(cancellationToken);
                
            return Result<IReadOnlyList<string>>.Success(interests.AsReadOnly());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get interests");
            return Result<IReadOnlyList<string>>.Failure(ErrorCodes.Validation.InvalidInput, "An error occurred while retrieving interests.");
        }
    }

    public async Task<Result> UpdateInterestsAsync(Guid userId, IReadOnlyList<string> interests, CancellationToken cancellationToken = default)
    {
        try
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));
            Guard.Against.Null(interests, nameof(interests));
            
            var user = await _context.Set<User>()
                .Include(u => u.Interests)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
                
            if (user is null)
            {
                return Result.NotFound(ErrorCodes.User.NotFound, $"User with ID '{userId}' was not found.");
            }
            
            // Clear existing interests
            user.Interests.Clear();
            
            // Add new interests
            foreach (var name in interests.Distinct())
            {
                var interest = await _context.Set<Interest>()
                    .FirstOrDefaultAsync(i => i.Name == name && !i.IsDeleted, cancellationToken);
                    
                if (interest is null)
                {
                    interest = new Interest
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Category = "General",
                        IsActive = true
                    };
                    await _context.Set<Interest>().AddAsync(interest, cancellationToken);
                }
                
                user.Interests.Add(new UserInterest
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    InterestId = interest.Id
                });
            }
            
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Interests updated for user: {UserId}", userId);
            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.ValidationError(ErrorCodes.Validation.InvalidInput, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update interests for user: {UserId}", userId);
            return Result.Failure(ErrorCodes.User.ProfileIncomplete, "An error occurred while updating interests.");
        }
    }
}