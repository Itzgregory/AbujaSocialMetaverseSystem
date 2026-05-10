using AbujaSocialMetaverse.Infrastructure.Data;
using AbujaSocialMetaverse.Shared.Models;

namespace AbujaSocialMetaverse.Modules.Core.Data.Entities;

public class User : BaseEntity
{
    // Authentication
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    
    // Identity
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    
    // Status
    public SocialMode CurrentMode { get; set; } = SocialMode.Leisure;
    public bool EmailVerified { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastActiveAt { get; set; }
    
    // Security
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTimeOffset? LockedUntil { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    
    // Navigation
    public ICollection<UserSetting> Settings { get; set; } = new List<UserSetting>();
    public ICollection<UserInterest> Interests { get; set; } = new List<UserInterest>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public UserDatingProfile? DatingProfile { get; set; }
    public UserNetworkingProfile? NetworkingProfile { get; set; }
    public ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; } = new List<EmailVerificationToken>();
}