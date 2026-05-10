namespace AbujaSocialMetaverse.Modules.Core.Internal.Templates;

/// <summary>
///     Represents the email template for email verification.
/// </summary>
public class VerificationEmailTemplate : BaseEmailTemplate
{
    public override string TemplateName => "EmailVerification";
    public override string Subject => "Verify Your Email Address - Abuja Social Metaverse";
    
    protected override string GetHtmlTemplate()
    {
        return @"<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { text-align: center; padding-bottom: 20px; border-bottom: 2px solid #eee; }
        .content { padding: 20px 0; }
        .button { display: inline-block; padding: 12px 24px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 4px; margin: 20px 0; }
        .footer { font-size: 12px; color: #777; text-align: center; padding-top: 20px; border-top: 1px solid #eee; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Abuja Social Metaverse</h2>
        </div>
        <div class='content'>
            <h3>Welcome, {{DisplayName}}!</h3>
            <p>Thank you for registering. Please verify your email address to start exploring the metaverse.</p>
            <p>Click the button below to verify your email:</p>
            <p style='text-align: center;'>
                <a href='{{VerificationLink}}' class='button'>Verify Email Address</a>
            </p>
            <p>Or copy and paste this link into your browser:</p>
            <p><code>{{VerificationLink}}</code></p>
            <p>This link will expire in 24 hours.</p>
        </div>
        <div class='footer'>
            <p>Abuja Social Metaverse - Connecting Abuja, One Avatar at a Time</p>
        </div>
    </div>
</body>
</html>";
    }
    
    protected override Dictionary<string, string> GetDefaultPlaceholders()
    {
        return new Dictionary<string, string>
        {
            { "DisplayName", "Valued User" },
            { "VerificationLink", "#" }
        };
    }
}