using MessagePack;

namespace AbujaSocialMetaverse.Infrastructure.RealTime.Models;

[MessagePackObject]
public class AvatarPositionUpdate
{
    [Key(0)] public Guid UserId { get; set; }
    [Key(1)] public float X { get; set; }
    [Key(2)] public float Y { get; set; }
    [Key(3)] public float Z { get; set; }
    [Key(4)] public float RotationY { get; set; }
    [Key(5)] public DateTimeOffset Timestamp { get; set; }
    [Key(6)] public string RegionId { get; set; } = string.Empty;
}