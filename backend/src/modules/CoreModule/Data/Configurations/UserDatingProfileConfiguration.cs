using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AbujaSocialMetaverse.Modules.Core.Data.Entities;

namespace AbujaSocialMetaverse.Modules.Core.Data.Configurations;

public class UserDatingProfileConfiguration : IEntityTypeConfiguration<UserDatingProfile>
{
    public void Configure(EntityTypeBuilder<UserDatingProfile> builder)
    {
        builder.ToTable("core_user_dating_profiles");
        
        builder.HasKey(p => p.Id);
        
        builder.HasIndex(p => p.UserId)
            .IsUnique();
        
        builder.Property(p => p.Gender)
            .HasMaxLength(20);
        
        builder.Property(p => p.GenderPreference)
            .HasMaxLength(20);
        
        builder.Property(p => p.DatingBio)
            .HasMaxLength(500);
        
        builder.HasOne(p => p.User)
            .WithOne(u => u.DatingProfile)
            .HasForeignKey<UserDatingProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}