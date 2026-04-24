using AbujaSocialMetaverse.Infrastructure.Data;
using AbujaSocialMetaverse.Modules.Core.Data.Entities;
using AbujaSocialMetaverse.Modules.Core.Internal.Mappers;
using AbujaSocialMetaverse.Modules.Core.Public.Interfaces;
using AbujaSocialMetaverse.Modules.Core.Public.Models;
using AbujaSocialMetaverse.Shared.Configuration.Options;
using AbujaSocialMetaverse.Shared.Constants;
using AbujaSocialMetaverse.Shared.Helpers;
using AbujaSocialMetaverse.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AbujaSocialMetaverse.Modules.Core.Internal.Services;

public class UserCreationService : BaseService, IUserCreationService
{
    private readonly UserOptions _userOptions;

    public UserCreationService(
        IUnitOfWork unitOfWork,
        IOptions<UserOptions> userOptions,
        ILogger<UserCreationService> logger)
        : base(logger, unitOfWork)
    {
        _userOptions = userOptions.Value;
    }

    public async Task<Result<UserDto>> CreateUserAsync(
        string email,
        string passwordHash,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(CreateUserAsync), async (ct) =>
        {
            Guard.Against.NullOrWhiteSpace(email, nameof(email));
            Guard.Against.NullOrWhiteSpace(passwordHash, nameof(passwordHash));
            Guard.Against.NullOrWhiteSpace(displayName, nameof(displayName));

            // Check if email already exists
            var existingUser = await _unitOfWork!.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, ct);

            if (existingUser is not null)
            {
                return Result<UserDto>.Conflict(
                    ErrorCodes.User.EmailAlreadyExists,
                    "User with this email already exists.");
            }

            // Create user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = passwordHash,
                DisplayName = Guard.Against.ExceedsMaxLength(
                    displayName,
                    nameof(displayName),
                    _userOptions.MaxDisplayNameLength),
                EmailVerified = false,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.Set<User>().AddAsync(user, ct);

            var saveResult = await SaveChangesAsync(ct);
            if (!saveResult.IsSuccess)
            {
                return Result<UserDto>.Failure(saveResult.Error!);
            }

            _logger.LogInformation("User created: {Email} with ID {UserId}", email, user.Id);

            return Result<UserDto>.Success(UserMapper.ToDto(user));
        }, cancellationToken);
    }
}