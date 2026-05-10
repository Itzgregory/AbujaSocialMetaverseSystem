namespace AbujaSocialMetaverse.Shared.Configuration.Options;

public class RecommendationOptions : FeatureOptions
{
    public override string SectionName => "Recommendation";

    /// <summary>
    /// Maximum number of recommendations returned per request.
    /// Per architecture docs: 20.
    /// </summary>
    public int MaxResults { get; set; } = 20;

    /// <summary>
    /// How long recommendation results are cached in minutes.
    /// Per optimization docs: 5-15 minutes.
    /// </summary>
    public int CacheTtlMinutes { get; set; } = 10;

    /// <summary>
    /// Default search radius in meters when user has not set a preference.
    /// Per optimization docs: 5km default.
    /// </summary>
    public int DefaultRadiusMeters { get; set; } = 5000;

    /// <summary>
    /// Maximum allowed search radius in meters.
    /// Prevents users from requesting city-wide recommendations in one call.
    /// </summary>
    public int MaxRadiusMeters { get; set; } = 20000;

    /// <summary>
    /// Minimum compatibility score (0-100) for a business to appear in recommendations.
    /// </summary>
    public int MinRelevanceScore { get; set; } = 10;

    public override void Validate()
    {
        if (MaxResults <= 0)
            throw new InvalidOperationException(
                $"[{SectionName}] MaxResults must be greater than 0.");

        if (CacheTtlMinutes <= 0)
            throw new InvalidOperationException(
                $"[{SectionName}] CacheTtlMinutes must be greater than 0.");

        if (DefaultRadiusMeters <= 0 || DefaultRadiusMeters > MaxRadiusMeters)
            throw new InvalidOperationException(
                $"[{SectionName}] DefaultRadiusMeters must be between 1 " +
                $"and {MaxRadiusMeters}.");
    }
}