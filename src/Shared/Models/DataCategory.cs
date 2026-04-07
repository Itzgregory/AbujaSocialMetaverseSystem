namespace AbujaSocialMetaverse.Shared.Models;

/// <summary>
/// Categories of personal data processed by the platform.
/// Used for consent management and retention enforcement.
/// Per NDPA 2023 compliance architecture.
/// </summary>
public enum DataCategory
{
    /// <summary>Name, email, phone — required for account operation.</summary>
    Identity = 0,

    /// <summary>Real-time coordinates, movement history. Retention: 30 days.</summary>
    Location = 1,

    /// <summary>Interactions, proximity events, compatibility checks. Retention: 12 months.</summary>
    SocialGraph = 2,

    /// <summary>Mode selections, business views, engagement patterns. Retention: 24 months.</summary>
    Behavioural = 3,

    /// <summary>Transaction records, subscription history. Retention: 7 years.</summary>
    Payment = 4,

    /// <summary>Flagged content, reports. Retention: 36 months.</summary>
    Moderation = 5
}