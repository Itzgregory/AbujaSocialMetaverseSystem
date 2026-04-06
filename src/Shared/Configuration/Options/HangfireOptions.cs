namespace AbujaSocialMetaverse.Shared.Configuration.Options;

public class HangfireOptions : FeatureOptions
{
    public override string SectionName => "Hangfire";

    public string DashboardPath { get; set; } = "/hangfire";

    /// <summary>
    /// Number of background worker threads.
    /// Default: ProcessorCount * 2 — scales with the host machine.
    /// </summary>
    public int WorkerCount { get; set; } = Environment.ProcessorCount * 2;

    /// <summary>
    /// Job queues in priority order.
    /// Critical: payment webhooks, erasure requests.
    /// Default: standard background jobs.
    /// Low: analytics, cache warming.
    /// </summary>
    public string[] Queues { get; set; } = ["critical", "default", "low"];

    /// <summary>
    /// How long to retain succeeded job records.
    /// Default: 1 day.
    /// </summary>
    public int SucceededJobRetentionHours { get; set; } = 24;

    /// <summary>
    /// How long to retain failed job records.
    /// Default: 7 days — gives time to investigate failures.
    /// </summary>
    public int FailedJobRetentionDays { get; set; } = 7;

    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(DashboardPath))
            throw new InvalidOperationException(
                $"[{SectionName}] DashboardPath is required.");

        if (WorkerCount <= 0)
            throw new InvalidOperationException(
                $"[{SectionName}] WorkerCount must be greater than 0.");

        if (Queues.Length == 0)
            throw new InvalidOperationException(
                $"[{SectionName}] At least one queue must be defined.");
    }
}