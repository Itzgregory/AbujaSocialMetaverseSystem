using AbujaSocialMetaverse.Infrastructure.Data;
using AbujaSocialMetaverse.Modules.Core.Data.Entities;
using AbujaSocialMetaverse.Modules.Core.Internal.Mappers;
using AbujaSocialMetaverse.Modules.Core.Public.Interfaces;
using AbujaSocialMetaverse.Modules.Core.Public.Models;
using AbujaSocialMetaverse.Shared.Constants;
using AbujaSocialMetaverse.Shared.Helpers;
using AbujaSocialMetaverse.Shared.Models;
using AbujaSocialMetaverse.Shared.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AbujaSocialMetaverse.Modules.Core.Internal.Services;

public class UserQueryService : BaseService, IUserQueryService
{
    public UserQueryService(
        IUnitOfWork unitOfWork,
        ILogger<UserQueryService> logger)
        : base(logger, unitOfWork)
    {
    }

    public async Task<Result<UserDto>> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(GetByIdAsync), async (ct) =>
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));

            var user = await _unitOfWork!.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct);

            if (user is null)
            {
                return Result<UserDto>.NotFound(
                    ErrorCodes.User.NotFound,
                    $"User with ID '{userId}' was not found.");
            }

            return Result<UserDto>.Success(UserMapper.ToDto(user));
        }, cancellationToken);
    }

    public async Task<Result<UserDto>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(GetByEmailAsync), async (ct) =>
        {
            Guard.Against.NullOrWhiteSpace(email, nameof(email));

            if (!CommonValidators.IsValidEmail(email))
            {
                return Result<UserDto>.ValidationError(
                    ErrorCodes.User.InvalidEmail,
                    "The provided email address is invalid.");
            }

            var user = await _unitOfWork!.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, ct);

            if (user is null)
            {
                return Result<UserDto>.NotFound(
                    ErrorCodes.User.NotFound,
                    $"User with email '{email}' was not found.");
            }

            return Result<UserDto>.Success(UserMapper.ToDto(user));
        }, cancellationToken);
    }
}