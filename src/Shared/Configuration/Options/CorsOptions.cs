namespace AbujaSocialMetaverse.Shared.Configuration.Options;

public class CorsOptions : FeatureOptions
{
    public override string SectionName => "Cors";

    /// <summary>
    /// Comma-separated list of allowed origins.
    /// Sourced from CORS_ALLOWED_ORIGINS in .env.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = [];

    /// <summary>
    /// Policy name used when registering CORS in the pipeline.
    /// </summary>
    public string PolicyName { get; set; } = "AllowUnityClient";

    /// <summary>
    /// Whether to allow credentials (required for SignalR).
    /// </summary>
    public bool AllowCredentials { get; set; } = true;

    public override void Validate()
    {
        if (AllowedOrigins.Length == 0)
            throw new InvalidOperationException(
                $"[{SectionName}] AllowedOrigins is required. " +
                $"Check CORS_ALLOWED_ORIGINS in your .env file.");
    }
}