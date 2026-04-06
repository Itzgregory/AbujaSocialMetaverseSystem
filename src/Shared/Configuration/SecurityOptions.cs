namespace AbujaSocialMetaverse.Shared.Configuration;

/// <summary>
/// Base for all options that involve secrets, keys, or auth configuration.
/// </summary>
public abstract class SecurityOptions : BaseOptions
{
    /// <summary>
    /// The primary secret key for this service.
    /// Must never be hardcoded — always sourced from .env.
    /// </summary>
    public abstract string SecretKey { get; set; }

    /// <summary>
    /// Token or session expiry in minutes.
    /// Default: 60 minutes.
    /// </summary>
    public int ExpiryMinutes { get; set; } = 60;

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(SecretKey))
            throw new InvalidOperationException(
                $"[{SectionName}] SecretKey is required but was not provided. " +
                $"Check your .env file.");

        if (SecretKey.Length < 32)
            throw new InvalidOperationException(
                $"[{SectionName}] SecretKey must be at least 32 characters long " +
                $"for adequate security.");
    }
}