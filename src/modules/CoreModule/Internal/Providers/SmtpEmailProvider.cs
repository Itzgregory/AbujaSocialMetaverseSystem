using System.Net;
using System.Net.Mail;
using AbujaSocialMetaverse.Modules.Core.Public.Interfaces;
using AbujaSocialMetaverse.Shared.Configuration.Options;
using AbujaSocialMetaverse.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AbujaSocialMetaverse.Modules.Core.Internal.Providers;

/// <summary>
///     Represents the email provider for sending emails via SMTP.
/// </summary>
public class SmtpEmailProvider : IEmailProvider
{
    private readonly EmailOptions _emailOptions;
    private readonly ILogger<SmtpEmailProvider> _logger;

    public string ProviderName => "Smtp";

    public SmtpEmailProvider(IOptions<EmailOptions> emailOptions, ILogger<SmtpEmailProvider> logger)
    {
        _emailOptions = emailOptions.Value;
        _logger = logger;
    }

    public async Task<Result> SendEmailAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new SmtpClient(_emailOptions.Host, _emailOptions.Port)
            {
                EnableSsl = _emailOptions.EnableSsl,
                Credentials = new NetworkCredential(_emailOptions.Username, _emailOptions.Password),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 30000
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_emailOptions.FromEmail, _emailOptions.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            
            message.To.Add(new MailAddress(toEmail, toName));

            await client.SendMailAsync(message, cancellationToken);
            
            _logger.LogInformation("Email sent to {ToEmail} via SMTP", toEmail);
            return Result.Success();
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error sending email to {ToEmail}", toEmail);
            return Result.Failure(ErrorCodes.Validation.InvalidInput, $"SMTP error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
            return Result.Failure(ErrorCodes.Validation.InvalidInput, "Failed to send email.");
        }
    }
}