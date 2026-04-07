using AbujaSocialMetaverse.Infrastructure.Data;
using AbujaSocialMetaverse.Modules.Core.Data.Entities;
using AbujaSocialMetaverse.Modules.Core.Helpers;
using AbujaSocialMetaverse.Modules.Core.Public;
using AbujaSocialMetaverse.Modules.Core.Public.Models;
using AbujaSocialMetaverse.Shared.Constants;
using AbujaSocialMetaverse.Shared.Helpers;
using AbujaSocialMetaverse.Shared.Models;
using AbujaSocialMetaverse.Shared.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AbujaSocialMetaverse.Modules.Core.Internal;

public class UserQueryService : IUserQueryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserQueryService> _logger;

    public UserQueryService(IUnitOfWork unitOfWork, ILogger<UserQueryService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UserDto>> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));
            
            var user = await _unitOfWork.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
                
            if (user is null)
            {
                return Result<UserDto>.NotFound(ErrorCodes.User.NotFound, $"User with ID '{userId}' was not found.");
            }
            
            var dto = UserMapper.ToDto(
                user.Id, user.Email, user.DisplayName, user.AvatarUrl, user.Bio,
                user.CurrentMode, user.CreatedAt, user.IsActive);
                
            return Result<UserDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user by ID: {UserId}", userId);
            return Result<UserDto>.Failure(ErrorCodes.User.NotFound, "An error occurred while retrieving the user.");
        }
    }

    public async Task<Result<UserDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            Guard.Against.NullOrWhiteSpace(email, nameof(email));
            
            if (!CommonValidators.IsValidEmail(email))
            {
                return Result<UserDto>.ValidationError(ErrorCodes.User.InvalidEmail, "The provided email address is invalid.");
            }
            
            var user = await _unitOfWork.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
                
            if (user is null)
            {
                return Result<UserDto>.NotFound(ErrorCodes.User.NotFound, $"User with email '{email}' was not found.");
            }
            
            var dto = UserMapper.ToDto(
                user.Id, user.Email, user.DisplayName, user.AvatarUrl, user.Bio,
                user.CurrentMode, user.CreatedAt, user.IsActive);
                
            return Result<UserDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user by email: {Email}", email);
            return Result<UserDto>.Failure(ErrorCodes.User.NotFound, "An error occurred while retrieving the user.");
        }
    }
}