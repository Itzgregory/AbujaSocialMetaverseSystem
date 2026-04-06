namespace AbujaSocialMetaverse.Infrastructure.BackgroundJobs;

/// <summary>
/// Centralised job ID constants for all recurring jobs.
/// Prevents magic strings across the codebase.
/// </summary>
public static class JobIds
{
    public const string DataRetentionPurge = "data-retention-purge";
    public const string RecommendationCacheWarm = "recommendation-cache-warm";
    public const string ActiveUserCleanup = "active-user-cleanup";
    public const string AnalyticsAggregation = "analytics-aggregation";
    public const string IndexMaintenance = "index-maintenance";
}