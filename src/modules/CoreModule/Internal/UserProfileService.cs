using AbujaSocialMetaverse.Infrastructure.Data;
using AbujaSocialMetaverse.Modules.Core.Data.Entities;
using AbujaSocialMetaverse.Modules.Core.Helpers;
using AbujaSocialMetaverse.Modules.Core.Public;
using AbujaSocialMetaverse.Modules.Core.Public.Models;
using AbujaSocialMetaverse.Shared.Configuration.Options;
using AbujaSocialMetaverse.Shared.Constants;
using AbujaSocialMetaverse.Shared.Helpers;
using AbujaSocialMetaverse.Shared.Models;
using AbujaSocialMetaverse.Shared.Validators;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AbujaSocialMetaverse.Modules.Core.Internal;

public class UserProfileService : IUserProfileService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserOptions _userOptions;
    private readonly ILogger<UserProfileService> _logger;

    public UserProfileService(
        IUnitOfWork unitOfWork,
        IOptions<UserOptions> userOptions,
        ILogger<UserProfileService> logger)
    {
        _unitOfWork = unitOfWork;  // ✅ Fixed: assign to _unitOfWork, not _context
        _userOptions = userOptions.Value;
        _logger = logger;
    }

    public async Task<Result<UserDto>> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));
            Guard.Against.Null(request, nameof(request));
            
            var user = await _unitOfWork.Set<User>()  
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
                
            if (user is null)
            {
                return Result<UserDto>.NotFound(
                    ErrorCodes.User.NotFound,
                    $"User with ID '{userId}' was not found.");
            }
            
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
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);  
            
            _logger.LogInformation("User profile updated: {UserId}", userId);
            
            var dto = UserMapper.ToDto(
                user.Id, user.Email, user.DisplayName, user.AvatarUrl, user.Bio,
                user.CurrentMode, user.CreatedAt, user.IsActive);
                
            return Result<UserDto>.Success(dto);
        }
        catch (ArgumentException ex)
        {
            return Result<UserDto>.ValidationError(ErrorCodes.Validation.InvalidInput, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update profile for user: {UserId}", userId);
            return Result<UserDto>.Failure(
                ErrorCodes.User.ProfileIncomplete,
                "An error occurred while updating the user profile.");
        }
    }

    public async Task<Result<UserSettingsDto>> UpdateSettingsAsync(
        Guid userId,
        UpdateSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));
            Guard.Against.Null(request, nameof(request));
            
            var user = await _unitOfWork.Set<User>()  
                .Include(u => u.Settings)
                .Include(u => u.Interests)
                    .ThenInclude(ui => ui.Interest)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
            
            if (user is null)
            {
                return Result<UserSettingsDto>.NotFound(
                    ErrorCodes.User.NotFound,
                    $"User with ID '{userId}' was not found.");
            }
            
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
                await UpdateInterestsAsync(user, request.Interests, cancellationToken);
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);  
            
            _logger.LogInformation("User settings updated: {UserId}", userId);
            
            var settingsDto = BuildSettingsDto(user);
            return Result<UserSettingsDto>.Success(settingsDto);
        }
        catch (ArgumentException ex)
        {
            return Result<UserSettingsDto>.ValidationError(ErrorCodes.Validation.InvalidInput, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update settings for user: {UserId}", userId);
            return Result<UserSettingsDto>.Failure(
                ErrorCodes.User.ProfileIncomplete,
                "An error occurred while updating user settings.");
        }
    }

    public async Task<Result> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
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
            
            var user = await _unitOfWork.Set<User>()  
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
                
            if (user is null)
            {
                return Result.NotFound(
                    ErrorCodes.User.NotFound,
                    $"User with ID '{userId}' was not found.");
            }
            
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                // Increment failed login attempts
                user.FailedLoginAttempts++;
                
                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockedUntil = DateTimeOffset.UtcNow.AddMinutes(15);
                }
                
                await _unitOfWork.SaveChangesAsync(cancellationToken);  
                
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
            
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                request.NewPassword,
                AppConstants.Security.BcryptWorkFactor);
                
            await _unitOfWork.SaveChangesAsync(cancellationToken);  
            
            _logger.LogInformation("Password changed for user: {UserId}", userId);
            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.ValidationError(ErrorCodes.Validation.InvalidInput, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change password for user: {UserId}", userId);
            return Result.Failure(
                ErrorCodes.User.InvalidPassword,
                "An error occurred while changing the password.");
        }
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
            var interest = await _unitOfWork.Set<Interest>()  
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