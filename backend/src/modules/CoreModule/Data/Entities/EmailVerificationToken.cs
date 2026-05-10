using AbujaSocialMetaverse.Infrastructure.Data;

namespace AbujaSocialMetaverse.Modules.Core.Data.Entities;

public class EmailVerificationToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    
    public bool IsValid => !IsUsed && ExpiresAt > DateTimeOffset.UtcNow && !IsDeleted;
    
    // Navigation
    public User? User { get; set; }
}