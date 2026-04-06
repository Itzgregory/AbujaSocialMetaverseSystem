namespace AbujaSocialMetaverse.Shared.Configuration.Options;

public class RealTimeOptions : FeatureOptions
{
    public override string SectionName => "RealTime";

    /// <summary>
    /// Distance in meters at which two avatars trigger a proximity event.
    /// Per architecture docs: 20 meters.
    /// </summary>
    public int ProximityThresholdMeters { get; set; } = 20;

    /// <summary>
    /// Size of each SignalR region group in meters.
    /// Per optimization docs: 200m x 200m grid cells.
    /// </summary>
    public int RegionSizeMeters { get; set; } = 200;

    /// <summary>
    /// Maximum number of remote avatars a single client receives updates for.
    /// Per optimization docs: 50 avatars per client.
    /// Directly coupled to concurrent user capacity per region.
    /// </summary>
    public int MaxAvatarsPerClient { get; set; } = 50;

    /// <summary>
    /// How frequently clients send position updates in milliseconds.
    /// Per optimization docs: 100ms (10Hz).
    /// </summary>
    public int PositionUpdateIntervalMs { get; set; } = 100;

    /// <summary>
    /// Number of static regions Abuja is divided into.
    /// Per architecture docs: 16 regions.
    /// </summary>
    public int TotalRegions { get; set; } = 16;

    /// <summary>
    /// Maximum distance in meters a client can see other avatars.
    /// Beyond this, no updates are sent.
    /// </summary>
    public int MaxVisibilityRadiusMeters { get; set; } = 300;

    public override void Validate()
    {
        if (ProximityThresholdMeters <= 0)
            throw new InvalidOperationException(
                $"[{SectionName}] ProximityThresholdMeters must be greater than 0.");

        if (RegionSizeMeters <= 0)
            throw new InvalidOperationException(
                $"[{SectionName}] RegionSizeMeters must be greater than 0.");

        if (MaxAvatarsPerClient <= 0)
            throw new InvalidOperationException(
                $"[{SectionName}] MaxAvatarsPerClient must be greater than 0.");

        if (PositionUpdateIntervalMs < 50)
            throw new InvalidOperationException(
                $"[{SectionName}] PositionUpdateIntervalMs must be at least 50ms " +
                $"to avoid overwhelming the server.");
    }
}