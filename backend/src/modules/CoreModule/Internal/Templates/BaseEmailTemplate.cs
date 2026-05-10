using AbujaSocialMetaverse.Modules.Core.Public.Interfaces;

namespace AbujaSocialMetaverse.Modules.Core.Internal.Templates;

/// <summary>
/// Represents the base class for email templates.
/// </summary>
public abstract class BaseEmailTemplate : IEmailTemplate
{
    public abstract string TemplateName { get; }
    public abstract string Subject { get; }
    
    protected abstract string GetHtmlTemplate();
    protected abstract Dictionary<string, string> GetDefaultPlaceholders();
    
    public string RenderHtmlBody(Dictionary<string, string> placeholders)
    {
        var html = GetHtmlTemplate();
        var allPlaceholders = GetDefaultPlaceholders();
        
        // Merge with provided placeholders (override defaults)
        foreach (var kvp in placeholders)
        {
            allPlaceholders[kvp.Key] = kvp.Value;
        }
        
        foreach (var kvp in allPlaceholders)
        {
            html = html.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
        }
        
        return html;
    }
}