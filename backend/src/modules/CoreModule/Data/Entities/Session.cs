using AbujaSocialMetaverse.Infrastructure.Data;

namespace AbujaSocialMetaverse.Modules.Core.Data.Entities;

public class Session : BaseEntity
{
    public Guid UserId { get; set; }
    public string Jti { get; set; } = string.Empty; // JWT ID
    public string RefreshToken { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokedReason { get; set; }
    public string UserAgent { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    
    public bool IsValid => !IsDeleted && RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;
    
    // Navigation
    public User? User { get; set; }
}