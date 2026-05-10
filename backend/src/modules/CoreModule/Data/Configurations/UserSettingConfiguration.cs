using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AbujaSocialMetaverse.Modules.Core.Data.Entities;

namespace AbujaSocialMetaverse.Modules.Core.Data.Configurations;

public class UserSettingConfiguration : IEntityTypeConfiguration<UserSetting>
{
    public void Configure(EntityTypeBuilder<UserSetting> builder)
    {
        builder.ToTable("core_user_settings");
        
        builder.HasKey(us => us.Id);
        
        builder.HasIndex(us => new { us.UserId, us.Key })
            .IsUnique();
        
        builder.Property(us => us.Key)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(us => us.Value)
            .IsRequired()
            .HasMaxLength(2000);
    }
}