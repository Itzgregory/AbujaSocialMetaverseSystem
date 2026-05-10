namespace AbujaSocialMetaverse.Infrastructure.BackgroundJobs;

/// <summary>
/// Centralised cron expressions for all recurring jobs.
/// </summary>
public static class CronSchedules
{
    /// <summary>Every day at 2:00 AM UTC — data retention purge.</summary>
    public const string DailyAt2Am = "0 2 * * *";

    /// <summary>Every hour — recommendation cache pre-warm.</summary>
    public const string Hourly = "0 * * * *";

    /// <summary>Every 5 minutes — active user cleanup.</summary>
    public const string EveryFiveMinutes = "*/5 * * * *";

    /// <summary>Every midnight — analytics aggregation.</summary>
    public const string Midnight = "0 0 * * *";

    /// <summary>Every Sunday at 3:00 AM UTC — index maintenance.</summary>
    public const string WeeklyAt3Am = "0 3 * * 0";
}