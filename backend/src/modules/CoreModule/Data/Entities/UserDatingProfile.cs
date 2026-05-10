using AbujaSocialMetaverse.Infrastructure.Data;

namespace AbujaSocialMetaverse.Modules.Core.Data.Entities;

public class UserDatingProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public DateTimeOffset DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string GenderPreference { get; set; } = string.Empty;
    public string? DatingBio { get; set; }
    
    // Navigation
    public User? User { get; set; }
}