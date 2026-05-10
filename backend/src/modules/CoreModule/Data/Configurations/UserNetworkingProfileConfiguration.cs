using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AbujaSocialMetaverse.Modules.Core.Data.Entities;

namespace AbujaSocialMetaverse.Modules.Core.Data.Configurations;

public class UserNetworkingProfileConfiguration : IEntityTypeConfiguration<UserNetworkingProfile>
{
    public void Configure(EntityTypeBuilder<UserNetworkingProfile> builder)
    {
        builder.ToTable("core_user_networking_profiles");
        
        builder.HasKey(p => p.Id);
        
        builder.HasIndex(p => p.UserId)
            .IsUnique();
        
        builder.Property(p => p.Industry)
            .HasMaxLength(100);
        
        builder.Property(p => p.Occupation)
            .HasMaxLength(100);
        
        builder.Property(p => p.Company)
            .HasMaxLength(200);
        
        builder.Property(p => p.NetworkingBio)
            .HasMaxLength(500);
        
        builder.HasOne(p => p.User)
            .WithOne(u => u.NetworkingProfile)
            .HasForeignKey<UserNetworkingProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}