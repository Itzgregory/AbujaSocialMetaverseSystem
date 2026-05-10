namespace AbujaSocialMetaverse.Shared.Configuration.Options;

/// <summary>
/// Represents configuration options for email services. 
/// Supports multiple providers (e.g., SMTP, SendGrid) and includes settings for connection, authentication, and email formatting. 
/// Validates required fields based on the selected provider to ensure proper configuration.
/// </summary>
public class EmailOptions : ConnectionOptions
{
    public override string SectionName => "Email";
    
    public override string ConnectionString => $"{Host}:{Port}";
    
    public string Provider { get; set; } = "Smtp";  // "Smtp", "SendGrid", "Mock"
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Abuja Social Metaverse";
    public bool EnableSsl { get; set; } = true;
    public string BaseUrl { get; set; } = string.Empty;  // For generating links
    
    public override void Validate()
    {
        if (string.Equals(Provider, "Mock", StringComparison.OrdinalIgnoreCase))
            return;

        if (Provider == "Smtp")
        {
            if (string.IsNullOrWhiteSpace(Host))
                throw new InvalidOperationException($"[{SectionName}] Host is required for SMTP provider.");
            if (Port <= 0 || Port > 65535)
                throw new InvalidOperationException($"[{SectionName}] Port must be between 1 and 65535.");
            if (string.IsNullOrWhiteSpace(Username))
                throw new InvalidOperationException($"[{SectionName}] Username is required for SMTP provider.");
            if (string.IsNullOrWhiteSpace(Password))
                throw new InvalidOperationException($"[{SectionName}] Password is required for SMTP provider.");
        }
        
        if (string.IsNullOrWhiteSpace(FromEmail))
            throw new InvalidOperationException($"[{SectionName}] FromEmail is required.");
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new InvalidOperationException($"[{SectionName}] BaseUrl is required for generating email links.");
    }
}