using AbujaSocialMetaverse.Infrastructure.Data;
using AbujaSocialMetaverse.Modules.Core.Data.Entities;
using AbujaSocialMetaverse.Modules.Core.Internal.Mappers;
using AbujaSocialMetaverse.Modules.Core.Public.Interfaces;
using AbujaSocialMetaverse.Modules.Core.Public.Models;
using AbujaSocialMetaverse.Shared.Configuration.Options;
using AbujaSocialMetaverse.Shared.Constants;
using AbujaSocialMetaverse.Shared.Exceptions; 
using AbujaSocialMetaverse.Shared.Helpers;
using AbujaSocialMetaverse.Shared.Models;
using AbujaSocialMetaverse.Shared.Validators;
using BCryptNet = BCrypt.Net.BCrypt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace AbujaSocialMetaverse.Modules.Core.Internal.Services;

public class UserProfileService : BaseService, IUserProfileService
{
    private readonly UserOptions _userOptions;
    private const string UserNotFoundMessage = "User not found.";

    public UserProfileService(
        IUnitOfWork unitOfWork,
        IOptions<UserOptions> userOptions,
        ILogger<UserProfileService> logger)
        : base(logger, unitOfWork)
    {
        _userOptions = userOptions.Value;
    }

    public async Task<Result<UserDto>> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(UpdateProfileAsync), async (ct) =>
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));
            Guard.Against.Null(request, nameof(request));

            var userResult = await GetUserByIdAsync(userId, ct);
            if (!userResult.IsSuccess || userResult.Value is null)
            {
                return Result<UserDto>.Failure(
                    userResult.Error ?? new ResultError(ErrorCodes.User.NotFound, UserNotFoundMessage, ErrorType.NotFound));
            }

            var user = userResult.Value!;

            if (!string.IsNullOrWhiteSpace(request.DisplayName))
            {
                user.DisplayName = Guard.Against.ExceedsMaxLength(
                    request.DisplayName,
                    nameof(request.DisplayName),
                    _userOptions.MaxDisplayNameLength);
            }

            if (!string.IsNullOrWhiteSpace(request.Bio))
            {
                user.Bio = Guard.Against.ExceedsMaxLength(
                    request.Bio,
                    nameof(request.Bio),
                    _userOptions.MaxBioLength);
            }

            if (request.AvatarUrl is not null)
            {
                user.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl)
                    ? null
                    : request.AvatarUrl;
            }

            var saveResult = await SaveChangesAsync(ct);
            if (!saveResult.IsSuccess)
            {
                return Result<UserDto>.Failure(saveResult.Error!);
            }

            _logger.LogInformation("User profile updated: {UserId}", userId);

            var dto = UserMapper.ToDto(user);
            return Result<UserDto>.Success(dto);
        }, cancellationToken);
    }

    public async Task<Result<UserSettingsDto>> UpdateSettingsAsync(
        Guid userId,
        UpdateSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(UpdateSettingsAsync), async (ct) =>
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));
            Guard.Against.Null(request, nameof(request));

            var userResult = await GetUserWithDetailsAsync(userId, ct);
            if (!userResult.IsSuccess || userResult.Value is null)
            {
                return Result<UserSettingsDto>.Failure(
                    userResult.Error ?? new ResultError(ErrorCodes.User.NotFound, UserNotFoundMessage, ErrorType.NotFound));
            }

            var user = userResult.Value!;

            // Update mode
            user.CurrentMode = request.CurrentMode;

            // Update settings using keys from config
            UpdateSetting(user, _userOptions.SettingKeyOpenToNetworking, request.OpenToNetworking.ToString());
            UpdateSetting(user, _userOptions.SettingKeyOpenToFriends, request.OpenToFriends.ToString());
            UpdateSetting(user, _userOptions.SettingKeyOpenToDating, request.OpenToDating.ToString());

            // Max travel radius - use request value or default from config
            var maxTravelRadius = request.MaxTravelRadiusMeters ?? _userOptions.DefaultTravelRadiusMeters;
            maxTravelRadius = Guard.Against.OutOfRange(
                maxTravelRadius,
                nameof(request.MaxTravelRadiusMeters),
                1,
                _userOptions.MaxTravelRadiusMeters);
            UpdateSetting(user, _userOptions.SettingKeyMaxTravelRadiusMeters, maxTravelRadius.ToString());

            // Min age preference - use request value or default from config
            var minAgePreference = request.MinAgePreference ?? _userOptions.MinAgePreference;
            minAgePreference = Guard.Against.OutOfRange(
                minAgePreference,
                nameof(request.MinAgePreference),
                _userOptions.MinAgePreference,
                _userOptions.MaxAgePreference);
            UpdateSetting(user, _userOptions.SettingKeyMinAgePreference, minAgePreference.ToString());

            // Max age preference - use request value or default from config
            var maxAgePreference = request.MaxAgePreference ?? _userOptions.MaxAgePreference;
            maxAgePreference = Guard.Against.OutOfRange(
                maxAgePreference,
                nameof(request.MaxAgePreference),
                minAgePreference,
                _userOptions.MaxAgePreference);
            UpdateSetting(user, _userOptions.SettingKeyMaxAgePreference, maxAgePreference.ToString());

            // Update interests
            if (request.Interests is not null)
            {
                await UpdateInterestsAsync(user, request.Interests, ct);
            }

            var saveResult = await SaveChangesAsync(ct);
            if (!saveResult.IsSuccess)
            {
                return Result<UserSettingsDto>.Failure(saveResult.Error!);
            }

            _logger.LogInformation("User settings updated: {UserId}", userId);

            var settingsDto = BuildSettingsDto(user);
            return Result<UserSettingsDto>.Success(settingsDto);
        }, cancellationToken);
    }

    public async Task<Result> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(ChangePasswordAsync), async (ct) =>
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));
            Guard.Against.Null(request, nameof(request));

            // Validate password length against config
            if (request.NewPassword.Length < _userOptions.MinPasswordLength ||
                request.NewPassword.Length > _userOptions.MaxPasswordLength)
            {
                return Result.ValidationError(
                    ErrorCodes.User.PasswordTooWeak,
                    $"Password must be between {_userOptions.MinPasswordLength} and {_userOptions.MaxPasswordLength} characters.");
            }

            var userResult = await GetUserByIdAsync(userId, ct);
            if (!userResult.IsSuccess || userResult.Value is null)
            {
                return Result.Failure(
                    userResult.Error ?? new ResultError(ErrorCodes.User.NotFound, UserNotFoundMessage, ErrorType.NotFound));
            }

            var user = userResult.Value!;

            if (!BCryptNet.Verify(request.CurrentPassword, user.PasswordHash))
            {
                // Increment failed login attempts
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockedUntil = DateTimeOffset.UtcNow.AddMinutes(15);
                }

                await SaveChangesAsync(ct);

                return Result.ValidationError(
                    ErrorCodes.User.InvalidPassword,
                    "Current password is incorrect.");
            }

            // Reset failed attempts on success
            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;

            var passwordIssues = CommonValidators.GetPasswordIssues(request.NewPassword);
            if (passwordIssues.Any())
            {
                return Result.ValidationError(
                    ErrorCodes.User.PasswordTooWeak,
                    string.Join(" ", passwordIssues));
            }

            user.PasswordHash = BCryptNet.HashPassword(
                request.NewPassword,
                AppConstants.Security.BcryptWorkFactor);

            return await SaveChangesAsync(ct);
        }, cancellationToken);
    }

    public async Task<Result> UpdatePasswordHashAsync(
        Guid userId,
        string newPasswordHash,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(UpdatePasswordHashAsync), async (ct) =>
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));
            Guard.Against.NullOrWhiteSpace(newPasswordHash, nameof(newPasswordHash));

            var userResult = await GetUserByIdAsync(userId, ct);
            if (!userResult.IsSuccess || userResult.Value is null)
            {
                return Result.Failure(
                    userResult.Error ?? new ResultError(ErrorCodes.User.NotFound, UserNotFoundMessage, ErrorType.NotFound));
            }

            var user = userResult.Value!;
            user.PasswordHash = newPasswordHash;

            return await SaveChangesAsync(ct);
        }, cancellationToken);
    }

    public async Task<Result> UpdateLastLoginInfoAsync(
        Guid userId,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(UpdateLastLoginInfoAsync), async (ct) =>
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));
            Guard.Against.NullOrWhiteSpace(ipAddress, nameof(ipAddress));

            var userResult = await GetUserByIdAsync(userId, ct);
            if (!userResult.IsSuccess || userResult.Value is null)
            {
                return Result.Failure(
                    userResult.Error ?? new ResultError(ErrorCodes.User.NotFound, UserNotFoundMessage, ErrorType.NotFound));
            }

            var user = userResult.Value!;
            user.LastLoginAt = DateTimeOffset.UtcNow;
            user.LastLoginIp = ipAddress;
            user.LastActiveAt = DateTimeOffset.UtcNow;

            return await SaveChangesAsync(ct);
        }, cancellationToken);
    }

    // Private helper methods
    private static void UpdateSetting(User user, string key, string value)
    {
        var setting = user.Settings.FirstOrDefault(s => s.Key == key);
        if (setting is null)
        {
            setting = new UserSetting
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Key = key,
                Value = value
            };
            user.Settings.Add(setting);
        }
        else
        {
            setting.Value = value;
        }
    }

    private async Task UpdateInterestsAsync(
        User user,
        IReadOnlyList<string> interestNames,
        CancellationToken cancellationToken)
    {
        user.Interests.Clear();

        foreach (var name in interestNames.Distinct())
        {
            var interest = await _unitOfWork!.Set<Interest>()
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
                await _unitOfWork.Set<Interest>().AddAsync(interest, cancellationToken);
            }

            user.Interests.Add(new UserInterest
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                InterestId = interest.Id
            });
        }
    }

    private UserSettingsDto BuildSettingsDto(User user)
    {
        var settings = user.Settings.ToDictionary(s => s.Key, s => s.Value);

        return new UserSettingsDto(
            OpenToNetworking: settings.GetValueOrDefault(_userOptions.SettingKeyOpenToNetworking) == "True",
            OpenToFriends: settings.GetValueOrDefault(_userOptions.SettingKeyOpenToFriends) == "True",
            OpenToDating: settings.GetValueOrDefault(_userOptions.SettingKeyOpenToDating) == "True",
            MaxTravelRadiusMeters: int.TryParse(
                settings.GetValueOrDefault(_userOptions.SettingKeyMaxTravelRadiusMeters), out var radius)
                ? radius
                : _userOptions.DefaultTravelRadiusMeters,
            MinAgePreference: int.TryParse(
                settings.GetValueOrDefault(_userOptions.SettingKeyMinAgePreference), out var minAge)
                ? minAge
                : _userOptions.MinAgePreference,
            MaxAgePreference: int.TryParse(
                settings.GetValueOrDefault(_userOptions.SettingKeyMaxAgePreference), out var maxAge)
                ? maxAge
                : _userOptions.MaxAgePreference,
            Interests: user.Interests.Select(ui => ui.Interest!.Name).ToList().AsReadOnly()
        );
    }
}