using AbujaSocialMetaverse.Modules.Core.Public.Interfaces;
using AbujaSocialMetaverse.Modules.Core.Internal.Templates;
using AbujaSocialMetaverse.Shared.Configuration.Options;
using AbujaSocialMetaverse.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AbujaSocialMetaverse.Modules.Core.Internal.Services;
/// <summary>
///     Represents the email service for sending emails.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IEnumerable<IEmailProvider> _providers;
    private readonly EmailOptions _emailOptions;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IEnumerable<IEmailProvider> providers,
        IOptions<EmailOptions> emailOptions,
        ILogger<EmailService> logger)
    {
        _providers = providers;
        _emailOptions = emailOptions.Value;
        _logger = logger;
    }

    private IEmailProvider GetProvider()
    {
        var provider = _providers.FirstOrDefault(p => p.ProviderName.Equals(_emailOptions.Provider, StringComparison.OrdinalIgnoreCase));
        
        if (provider is null)
        {
            _logger.LogWarning("Email provider {Provider} not found, falling back to first available", _emailOptions.Provider);
            provider = _providers.FirstOrDefault();
        }
        
        return provider ?? throw new InvalidOperationException("No email provider registered.");
    }

    public async Task<Result> SendVerificationEmailAsync(string toEmail, string displayName, string verificationLink, CancellationToken cancellationToken = default)
    {
        var template = new VerificationEmailTemplate();
        var placeholders = new Dictionary<string, string>
        {
            { "DisplayName", displayName },
            { "VerificationLink", verificationLink }
        };
        
        var htmlBody = template.RenderHtmlBody(placeholders);
        var provider = GetProvider();
        
        return await provider.SendEmailAsync(toEmail, displayName, template.Subject, htmlBody, cancellationToken);
    }

    public async Task<Result> SendPasswordResetEmailAsync(string toEmail, string displayName, string resetLink, CancellationToken cancellationToken = default)
    {
        var template = new PasswordResetEmailTemplate();
        var placeholders = new Dictionary<string, string>
        {
            { "DisplayName", displayName },
            { "ResetLink", resetLink }
        };
        
        var htmlBody = template.RenderHtmlBody(placeholders);
        var provider = GetProvider();
        
        return await provider.SendEmailAsync(toEmail, displayName, template.Subject, htmlBody, cancellationToken);
    }
}