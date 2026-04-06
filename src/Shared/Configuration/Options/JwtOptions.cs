namespace AbujaSocialMetaverse.Shared.Configuration.Options;

public class JwtOptions : SecurityOptions
{
    public override string SectionName => "Jwt";

    public override string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int RefreshExpiryDays { get; set; } = 7;

    /// <summary>
    /// Whether to allow token refresh within this many minutes before expiry.
    /// Default: 5 minutes.
    /// </summary>
    public int RefreshWindowMinutes { get; set; } = 5;

    public override void Validate()
    {
        base.Validate();

        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException(
                $"[{SectionName}] Issuer is required. Check JWT_ISSUER in your .env file.");

        if (string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException(
                $"[{SectionName}] Audience is required. Check JWT_AUDIENCE in your .env file.");

        if (ExpiryMinutes <= 0)
            throw new InvalidOperationException(
                $"[{SectionName}] ExpiryMinutes must be greater than 0.");

        if (RefreshExpiryDays <= 0)
            throw new InvalidOperationException(
                $"[{SectionName}] RefreshExpiryDays must be greater than 0.");
    }
}