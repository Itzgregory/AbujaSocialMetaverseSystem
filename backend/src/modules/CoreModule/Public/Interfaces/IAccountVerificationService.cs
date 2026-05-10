using AbujaSocialMetaverse.Shared.Models;

namespace AbujaSocialMetaverse.Modules.Core.Public.Interfaces;

/// <summary>
/// Service responsible for email verification and password reset workflows.
/// Manages verification token lifecycle, email dispatch, and credential updates.
/// </summary>
public interface IAccountVerificationService
{
    /// <summary>
    /// Generates a verification token and sends a verification email to the user.
    /// </summary>
    /// <param name="userId">The ID of the user requesting email verification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> RequestEmailVerificationAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a verification token and marks the user's email as verified.
    /// </summary>
    /// <param name="token">The email verification token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing true if the email was successfully verified.</returns>
    Task<Result<bool>> ConfirmEmailAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a password reset token and sends a reset email to the user.
    /// Returns success even if the email doesn't exist (security best practice).
    /// </summary>
    /// <param name="email">The email address of the user requesting a password reset.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a reset token, updates the user's password, and revokes all active sessions.
    /// </summary>
    /// <param name="token">The password reset token.</param>
    /// <param name="newPassword">The new password to set.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default);
}
