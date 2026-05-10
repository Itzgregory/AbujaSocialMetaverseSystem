using AbujaSocialMetaverse.Shared.Models;

namespace AbujaSocialMetaverse.Modules.Core.Public.Interfaces;

/// <summary>
/// Represents an email provider for sending emails.
/// </summary>
public interface IEmailProvider
{
    string ProviderName { get; }
    Task<Result> SendEmailAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken cancellationToken = default);
}