using AbujaSocialMetaverse.Infrastructure.Data;

namespace AbujaSocialMetaverse.Modules.Core.Data.Entities;

public class UserNetworkingProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string Industry { get; set; } = string.Empty;
    public string Occupation { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? NetworkingBio { get; set; }
    
    // Navigation
    public User? User { get; set; }
}