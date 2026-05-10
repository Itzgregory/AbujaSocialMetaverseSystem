using AbujaSocialMetaverse.Infrastructure.Data;

namespace AbujaSocialMetaverse.Modules.Core.Data.Entities;

public class UserInterest : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid InterestId { get; set; }
    
    // Navigation
    public User? User { get; set; }
    public Interest? Interest { get; set; }
}