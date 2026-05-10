using AbujaSocialMetaverse.Shared.Configuration.Options;
using AbujaSocialMetaverse.Shared.Contracts;
using Microsoft.Extensions.Options;

namespace AbujaSocialMetaverse.API.Services;

/// <summary>
/// Implementation of IEmailLinkGenerator interface for generating email links.
/// </summary>
public class EmailLinkGenerator : IEmailLinkGenerator
{
    private readonly EmailOptions _emailOptions;

    public EmailLinkGenerator(IOptions<EmailOptions> emailOptions)
    {
        _emailOptions = emailOptions.Value;
    }

    public string GenerateVerificationLink(string token)
    {
        return $"{_emailOptions.BaseUrl}/api/auth/verify-email?token={Uri.EscapeDataString(token)}";
    }

    public string GeneratePasswordResetLink(string token)
    {
        return $"{_emailOptions.BaseUrl}/api/auth/reset-password?token={Uri.EscapeDataString(token)}";
    }
}