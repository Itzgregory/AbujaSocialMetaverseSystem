using AbujaSocialMetaverse.Modules.Core.Public.Interfaces;
using AbujaSocialMetaverse.Infrastructure.Data;
using AbujaSocialMetaverse.Modules.Core.Data.Entities;
using AbujaSocialMetaverse.Shared.Constants;
using AbujaSocialMetaverse.Shared.Contracts;
using AbujaSocialMetaverse.Shared.Exceptions;
using AbujaSocialMetaverse.Shared.Helpers;
using AbujaSocialMetaverse.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AbujaSocialMetaverse.Modules.Core.Internal.Services;

/// <summary>
/// Handles email verification and password reset workflows.
/// Manages verification token lifecycle, email dispatch, and credential updates.
/// </summary>
public class AccountVerificationService : BaseService, IAccountVerificationService
{
    private readonly IPasswordService _passwordService;
    private readonly ISessionService _sessionService;
    private readonly IEmailService _emailService;
    private readonly IEmailLinkGenerator _emailLinkGenerator;

    public AccountVerificationService(
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        ISessionService sessionService,
        IEmailService emailService,
        IEmailLinkGenerator emailLinkGenerator,
        ILogger<AccountVerificationService> logger)
        : base(logger, unitOfWork)
    {
        _passwordService = passwordService;
        _sessionService = sessionService;
        _emailService = emailService;
        _emailLinkGenerator = emailLinkGenerator;
    }

    public async Task<Result> RequestEmailVerificationAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(RequestEmailVerificationAsync), async (ct) =>
        {
            Guard.Against.EmptyGuid(userId, nameof(userId));

            var userResult = await GetUserByIdAsync(userId, ct);
            if (!userResult.IsSuccess || userResult.Value is null)
            {
                return Result.Failure(userResult.Error ?? new ResultError(ErrorCodes.User.NotFound, "User not found.", ErrorType.NotFound));
            }

            var user = userResult.Value;

            if (user.EmailVerified)
            {
                return Result.ValidationError(ErrorCodes.Auth.EmailNotVerified, "Email already verified.");
            }

            var token = await CreateTokenAsync(user.Id, TimeSpan.FromHours(24), ct);

            // Generate verification link and send email
            var verificationLink = _emailLinkGenerator.GenerateVerificationLink(token);
            var emailResult = await _emailService.SendVerificationEmailAsync(user.Email, user.DisplayName, verificationLink, ct);
            
            if (!emailResult.IsSuccess)
            {
                return emailResult;
            }

            _logger.LogInformation("Verification email sent to user {UserId} at {Email}", user.Id, user.Email);
            return Result.Success();
        }, cancellationToken);
    }

    public async Task<Result<bool>> ConfirmEmailAsync(string token, CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(ConfirmEmailAsync), async (ct) =>
        {
            Guard.Against.NullOrWhiteSpace(token, nameof(token));

            var verificationToken = await FindValidTokenAsync(token, ct);
            if (verificationToken is null)
            {
                return Result<bool>.ValidationError(ErrorCodes.Auth.TokenInvalid, "Invalid or expired verification token.");
            }

            var userResult = await GetUserByIdAsync(verificationToken.UserId, ct);
            if (!userResult.IsSuccess || userResult.Value is null)
            {
                return Result<bool>.Failure(userResult.Error!);
            }

            var user = userResult.Value;
            user.EmailVerified = true;

            MarkTokenAsUsed(verificationToken);

            await SaveChangesAsync(ct);

            _logger.LogInformation("Email verified for user {UserId}", user.Id);
            return Result<bool>.Success(true);
        }, cancellationToken);
    }

    public async Task<Result> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(ForgotPasswordAsync), async (ct) =>
        {
            Guard.Against.NullOrWhiteSpace(email, nameof(email));

            var userResult = await GetUserByEmailAsync(email, ct);
            if (!userResult.IsSuccess || userResult.Value is null)
            {
                // For security, don't reveal if email exists
                _logger.LogInformation("Password reset requested for non-existent email {Email}", email);
                return Result.Success();
            }

            var user = userResult.Value;

            var token = await CreateTokenAsync(user.Id, TimeSpan.FromHours(1), ct);

            // Generate reset link and send email
            var resetLink = _emailLinkGenerator.GeneratePasswordResetLink(token);
            var emailResult = await _emailService.SendPasswordResetEmailAsync(user.Email, user.DisplayName, resetLink, ct);
            
            if (!emailResult.IsSuccess)
            {
                return emailResult;
            }

            _logger.LogInformation("Password reset email sent to user {UserId} at {Email}", user.Id, user.Email);
            return Result.Success();
        }, cancellationToken);
    }

    public async Task<Result> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(nameof(ResetPasswordAsync), async (ct) =>
        {
            Guard.Against.NullOrWhiteSpace(token, nameof(token));
            Guard.Against.NullOrWhiteSpace(newPassword, nameof(newPassword));

            var resetToken = await FindValidTokenAsync(token, ct);
            if (resetToken is null)
            {
                return Result.ValidationError(ErrorCodes.Auth.TokenInvalid, "Invalid or expired reset token.");
            }

            var userResult = await GetUserByIdAsync(resetToken.UserId, ct);
            if (!userResult.IsSuccess || userResult.Value is null)
            {
                return Result.Failure(userResult.Error!);
            }

            var user = userResult.Value;

            // Validate and hash new password
            var hashResult = ValidateAndHashPassword(newPassword);
            if (!hashResult.IsSuccess || string.IsNullOrWhiteSpace(hashResult.Value))
            {
                return Result.Failure(hashResult.Error!);
            }

            user.PasswordHash = hashResult.Value;
            
            MarkTokenAsUsed(resetToken);

            // Revoke all user sessions (force re-login)
            await _sessionService.RevokeAllUserSessionsAsync(user.Id, ct);
            
            await SaveChangesAsync(ct);

            _logger.LogInformation("Password reset for user {UserId}", user.Id);
            return Result.Success();
        }, cancellationToken);
    }

    // ─── Private Helpers ──────────────────────────────────────────────

    /// <summary>
    /// Creates a verification/reset token entity and persists it.
    /// Eliminates duplication between email verification and password reset token creation.
    /// </summary>
    private async Task<string> CreateTokenAsync(Guid userId, TimeSpan expiry, CancellationToken ct)
    {
        var tokenValue = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = tokenValue,
            ExpiresAt = DateTimeOffset.UtcNow.Add(expiry),
            IsUsed = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _unitOfWork!.Set<EmailVerificationToken>().AddAsync(verificationToken, ct);
        await SaveChangesAsync(ct);

        return tokenValue;
    }

    /// <summary>
    /// Finds a valid (unused, not expired, not deleted) token.
    /// Eliminates duplication between ConfirmEmail and ResetPassword token lookups.
    /// </summary>
    private async Task<EmailVerificationToken?> FindValidTokenAsync(string token, CancellationToken ct)
    {
        return await _unitOfWork!.Set<EmailVerificationToken>()
            .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && !t.IsDeleted && t.ExpiresAt > DateTimeOffset.UtcNow, ct);
    }

    /// <summary>
    /// Validates password strength and returns the hash.
    /// Eliminates duplication between registration and password reset flows.
    /// </summary>
    private Result<string> ValidateAndHashPassword(string password)
    {
        var strengthResult = _passwordService.ValidateStrength(password);
        if (!strengthResult.IsSuccess)
        {
            return Result<string>.ValidationError(ErrorCodes.User.PasswordTooWeak, "Password does not meet security requirements.");
        }

        var hashResult = _passwordService.HashPassword(password);
        if (!hashResult.IsSuccess || string.IsNullOrWhiteSpace(hashResult.Value))
        {
            return Result<string>.Failure(ErrorCodes.Validation.InternalError, "Failed to hash password.");
        }

        return Result<string>.Success(hashResult.Value);
    }

    /// <summary>
    /// Marks a token as used with timestamp update.
    /// </summary>
    private static void MarkTokenAsUsed(EmailVerificationToken token)
    {
        token.IsUsed = true;
        token.UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Retrieves a user by email with existence check.
    /// </summary>
    private async Task<Result<User>> GetUserByEmailAsync(string email, CancellationToken ct)
    {
        if (_unitOfWork is null)
        {
            return Result<User>.Failure(ErrorCodes.Validation.InternalError, "Unit of work not available.");
        }

        var user = await _unitOfWork.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, ct);

        if (user is null)
        {
            return Result<User>.NotFound(ErrorCodes.User.NotFound, "User not found.");
        }

        return Result<User>.Success(user);
    }
}
