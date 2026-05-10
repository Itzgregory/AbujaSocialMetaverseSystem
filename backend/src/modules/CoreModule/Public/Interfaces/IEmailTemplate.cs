namespace AbujaSocialMetaverse.Modules.Core.Public.Interfaces;

/// <summary>
/// Represents an email template for generating email content.
/// </summary>
public interface IEmailTemplate
{
    string TemplateName { get; }
    string Subject { get; }
    string RenderHtmlBody(Dictionary<string, string> placeholders);
}