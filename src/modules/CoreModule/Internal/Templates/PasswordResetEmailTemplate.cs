namespace AbujaSocialMetaverse.Modules.Core.Internal.Templates;

/// <summary>
///   Represents the email template for password reset.
/// </summary>
public class PasswordResetEmailTemplate : BaseEmailTemplate
{
    public override string TemplateName => "PasswordReset";
    public override string Subject => "Reset Your Password - Abuja Social Metaverse";
    
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
        .button { display: inline-block; padding: 12px 24px; background-color: #ff5722; color: white; text-decoration: none; border-radius: 4px; margin: 20px 0; }
        .warning { background-color: #fff3e0; padding: 15px; border-left: 4px solid #ff5722; margin: 20px 0; }
        .footer { font-size: 12px; color: #777; text-align: center; padding-top: 20px; border-top: 1px solid #eee; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Abuja Social Metaverse</h2>
        </div>
        <div class='content'>
            <h3>Password Reset Request</h3>
            <p>Hello {{DisplayName}},</p>
            <p>We received a request to reset your password. Click the button below to create a new password:</p>
            <p style='text-align: center;'>
                <a href='{{ResetLink}}' class='button'>Reset Password</a>
            </p>
            <p>Or copy and paste this link into your browser:</p>
            <p><code>{{ResetLink}}</code></p>
            <div class='warning'>
                <strong>⚠️ Security Notice:</strong> This link will expire in 1 hour. If you didn't request this, please ignore this email and your password will remain unchanged.
            </div>
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
            { "ResetLink", "#" }
        };
    }
}