namespace AbujaSocialMetaverse.Shared.Configuration;

/// <summary>
/// Base for all options that configure platform behaviour and features.
/// </summary>
public abstract class FeatureOptions : BaseOptions
{
    /// <summary>
    /// Whether this feature is enabled.
    /// Allows features to be toggled via configuration without code changes.
    /// Default: true.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    public override void Validate()
    {
        // Base validation — subclasses extend this
    }
}