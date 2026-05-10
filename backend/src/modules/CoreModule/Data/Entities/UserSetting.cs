using AbujaSocialMetaverse.Infrastructure.Data;
using AbujaSocialMetaverse.Shared.Models;

namespace AbujaSocialMetaverse.Modules.Core.Data.Entities;

public class UserSetting : BaseEntity
{
    public Guid UserId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    
    // Navigation
    public User? User { get; set; }
}