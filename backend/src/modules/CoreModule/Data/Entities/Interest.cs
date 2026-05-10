using AbujaSocialMetaverse.Infrastructure.Data;

namespace AbujaSocialMetaverse.Modules.Core.Data.Entities;

public class Interest : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    // Navigation
    public ICollection<UserInterest> UserInterests { get; set; } = new List<UserInterest>();
}