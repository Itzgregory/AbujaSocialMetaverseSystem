using AbujaSocialMetaverse.Shared.Models;

namespace AbujaSocialMetaverse.Modules.Core.Public.Interfaces;

public interface IEmailService
{
    ///<summary>
    /// Send verification email methods    
    ///</summary>
    Task<Result> SendVerificationEmailAsync(string toEmail, string displayName, string verificationLink, CancellationToken cancellationToken = default);
    ///<summary>
    /// Send password reset email methods    
    ///</summary>
    Task<Result> SendPasswordResetEmailAsync(string toEmail, string displayName, string resetLink, CancellationToken cancellationToken = default);
}