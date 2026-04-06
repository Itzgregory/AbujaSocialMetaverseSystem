namespace AbujaSocialMetaverse.Shared.Configuration.Options;

public class PrivacyOptions : FeatureOptions
{
    public override string SectionName => "Privacy";

    /// <summary>
    /// How long location history is retained in days.
    /// Per compliance docs: 30 days rolling.
    /// </summary>
    public int LocationRetentionDays { get; set; } = 30;

    /// <summary>
    /// How long social graph data is retained in days.
    /// Per compliance docs: 12 months.
    /// </summary>
    public int SocialGraphRetentionDays { get; set; } = 365;

    /// <summary>
    /// How long behavioural analytics are retained in days.
    /// Per compliance docs: 24 months, anonymized.
    /// </summary>
    public int BehaviouralRetentionDays { get; set; } = 730;

    /// <summary>
    /// How long payment records are retained in days.
    /// Per NDPA 2023 legal obligation: 7 years.
    /// </summary>
    public int PaymentRetentionDays { get; set; } = 2555;

    /// <summary>
    /// Maximum hours to complete a data erasure request.
    /// Per NDPA 2023: 72 hours.
    /// </summary>
    public int ErasureSlaHours { get; set; } = 72;

    /// <summary>
    /// Whether audit logging is enforced for all personal data operations.
    /// Must always be true in production.
    /// </summary>
    public bool EnforceAuditLogging { get; set; } = true;

    public override void Validate()
    {
        if (LocationRetentionDays <= 0)
            throw new InvalidOperationException(
                $"[{SectionName}] LocationRetentionDays must be greater than 0.");

        if (ErasureSlaHours <= 0 || ErasureSlaHours > 72)
            throw new InvalidOperationException(
                $"[{SectionName}] ErasureSlaHours must be between 1 and 72 " +
                $"per NDPA 2023 requirements.");

        if (!EnforceAuditLogging)
            throw new InvalidOperationException(
                $"[{SectionName}] EnforceAuditLogging cannot be disabled. " +
                $"Audit logging is required for NDPA 2023 compliance.");
    }
}