namespace AbujaSocialMetaverse.Shared.Configuration.Options;

public class RateLimitOptions : FeatureOptions
{
    public override string SectionName => "RateLimit";

    /// <summary>
    /// Maximum number of requests allowed per window.
    /// </summary>
    public int PermitLimit { get; set; } = 100;

    /// <summary>
    /// Duration of the rate limit window in seconds.
    /// </summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// Maximum number of requests queued when limit is reached.
    /// </summary>
    public int QueueLimit { get; set; } = 10;

    /// <summary>
    /// Stricter limit for auth endpoints (login, register, refresh).
    /// Prevents brute force attacks.
    /// Default: 10 requests per window.
    /// </summary>
    public int AuthEndpointPermitLimit { get; set; } = 10;

    /// <summary>
    /// Stricter limit for SignalR hub connections per user.
    /// </summary>
    public int SignalRPermitLimit { get; set; } = 5;

    public override void Validate()
    {
        if (PermitLimit <= 0)
            throw new InvalidOperationException(
                $"[{SectionName}] PermitLimit must be greater than 0.");

        if (WindowSeconds <= 0)
            throw new InvalidOperationException(
                $"[{SectionName}] WindowSeconds must be greater than 0.");

        if (QueueLimit < 0)
            throw new InvalidOperationException(
                $"[{SectionName}] QueueLimit cannot be negative.");

        if (AuthEndpointPermitLimit <= 0)
            throw new InvalidOperationException(
                $"[{SectionName}] AuthEndpointPermitLimit must be greater than 0.");
    }
}