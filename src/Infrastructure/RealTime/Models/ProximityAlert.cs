namespace AbujaSocialMetaverse.Infrastructure.RealTime.Models;

public class ProximityAlert
{
    public Guid UserAId { get; set; }
    public Guid UserBId { get; set; }
    public float DistanceMeters { get; set; }
    public string RegionId { get; set; } = string.Empty;
    public DateTimeOffset DetectedAt { get; set; } = DateTimeOffset.UtcNow;
}