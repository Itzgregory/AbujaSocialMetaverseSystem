using System.Text.RegularExpressions;
using AbujaSocialMetaverse.Modules.Core.Public.Interfaces;
using AbujaSocialMetaverse.Shared.Configuration.Options;
using AbujaSocialMetaverse.Shared.Constants;
using AbujaSocialMetaverse.Shared.Helpers;
using AbujaSocialMetaverse.Shared.Models;
using BCryptNet = BCrypt.Net.BCrypt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AbujaSocialMetaverse.Modules.Core.Internal.Services;

public class PasswordService : BaseService, IPasswordService
{
    private readonly PasswordPolicyOptions _passwordPolicyOptions;

    public PasswordService(
        ILogger<PasswordService> logger,
        IOptions<PasswordPolicyOptions> passwordPolicyOptions)
        : base(logger, null)
    {
        _passwordPolicyOptions = passwordPolicyOptions.Value;
    }

    public Result<string> HashPassword(string password)
    {
        return ExecuteSync(nameof(HashPassword), () =>
        {
            Guard.Against.NullOrWhiteSpace(password, nameof(password));

            var hash = BCryptNet.HashPassword(password, AppConstants.Security.BcryptWorkFactor);
            _logger.LogDebug("Password hashed successfully");
            return Result<string>.Success(hash);
        });
    }

    public Result<bool> VerifyPassword(string password, string hash)
    {
        return ExecuteSync(nameof(VerifyPassword), () =>
        {
            Guard.Against.NullOrWhiteSpace(password, nameof(password));
            Guard.Against.NullOrWhiteSpace(hash, nameof(hash));

            var isValid = BCryptNet.Verify(password, hash);

            if (!isValid)
            {
                _logger.LogWarning("Password verification failed");
            }
            else
            {
                _logger.LogDebug("Password verification successful");
            }

            return Result<bool>.Success(isValid);
        });
    }

    public Result ValidateStrength(string password)
    {
        return ExecuteSync(nameof(ValidateStrength), () =>
        {
            Guard.Against.NullOrWhiteSpace(password, nameof(password));

            var checks = new List<Func<Result>>
            {
                () => CheckLength(password),
                () => CheckUppercase(password),
                () => CheckLowercase(password),
                () => CheckDigit(password),
                () => CheckSpecialCharacter(password)
            };

            foreach (var check in checks)
            {
                var result = check();
                if (!result.IsSuccess)
                {
                    return result;
                }
            }

            _logger.LogDebug("Password strength validation passed");
            return Result.Success();
        });
    }

    public Result<bool> NeedsRehash(string hash, int currentWorkFactor)
    {
        return ExecuteSync(nameof(NeedsRehash), () =>
        {
            Guard.Against.NullOrWhiteSpace(hash, nameof(hash));

            var match = Regex.Match(hash, @"^\$2[aby]?\$(\d+)\$");
            if (!match.Success)
            {
                _logger.LogWarning("Unable to parse work factor from hash: invalid format");
                return Result<bool>.Success(false);
            }

            var storedWorkFactor = int.Parse(match.Groups[1].Value);
            var needsRehash = storedWorkFactor != currentWorkFactor;

            if (needsRehash)
            {
                _logger.LogInformation(
                    "Password hash needs rehash: stored work factor {Stored}, current {Current}",
                    storedWorkFactor, currentWorkFactor);
            }

            return Result<bool>.Success(needsRehash);
        });
    }

    private Result CheckLength(string password)
    {
        if (password.Length < _passwordPolicyOptions.MinLength)
        {
            return Result.ValidationError(
                ErrorCodes.User.PasswordTooWeak,
                $"Password must be at least {_passwordPolicyOptions.MinLength} characters.");
        }

        if (password.Length > _passwordPolicyOptions.MaxLength)
        {
            return Result.ValidationError(
                ErrorCodes.User.PasswordTooWeak,
                $"Password cannot exceed {_passwordPolicyOptions.MaxLength} characters.");
        }

        return Result.Success();
    }

    private static Result CheckUppercase(string password)
    {
        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            return Result.ValidationError(
                ErrorCodes.User.PasswordTooWeak,
                "Password must contain at least one uppercase letter.");
        }
        return Result.Success();
    }

    private static Result CheckLowercase(string password)
    {
        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            return Result.ValidationError(
                ErrorCodes.User.PasswordTooWeak,
                "Password must contain at least one lowercase letter.");
        }
        return Result.Success();
    }

    private static Result CheckDigit(string password)
    {
        if (!Regex.IsMatch(password, @"[0-9]"))
        {
            return Result.ValidationError(
                ErrorCodes.User.PasswordTooWeak,
                "Password must contain at least one digit.");
        }
        return Result.Success();
    }

    private static Result CheckSpecialCharacter(string password)
    {
        if (!Regex.IsMatch(password, @"[\W_]"))
        {
            return Result.ValidationError(
                ErrorCodes.User.PasswordTooWeak,
                "Password must contain at least one special character.");
        }
        return Result.Success();
    }
}