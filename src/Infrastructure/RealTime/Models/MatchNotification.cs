namespace AbujaSocialMetaverse.Infrastructure.RealTime.Models;

public class MatchNotification
{
    public Guid MatchedUserId { get; set; }
    public string MatchedUserDisplayName { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public float CompatibilityScore { get; set; }
    public DateTimeOffset IssuedAt { get; set; } = DateTimeOffset.UtcNow;
}