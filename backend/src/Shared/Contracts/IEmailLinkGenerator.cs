namespace AbujaSocialMetaverse.Shared.Contracts;

/// <summary>
/// Interface for generating email links.
/// </summary>
public interface IEmailLinkGenerator
{
    /// <summary>
    /// Generates a verification link.
    /// </summary>
    /// <param name="token">The verification token.</param>
    /// <returns>The verification link.</returns>
    string GenerateVerificationLink(string token);

    /// <summary>
    /// Generates a password reset link.
    /// </summary>
    /// <param name="token">The reset token.</param>
    /// <returns>The reset link.</returns>
    string GeneratePasswordResetLink(string token);
}