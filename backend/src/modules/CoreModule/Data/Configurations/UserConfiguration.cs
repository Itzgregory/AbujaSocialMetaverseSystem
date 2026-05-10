using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AbujaSocialMetaverse.Shared.Models;
using AbujaSocialMetaverse.Modules.Core.Data.Entities;

namespace AbujaSocialMetaverse.Modules.Core.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("core_users");
        
        builder.HasKey(u => u.Id);
        
        builder.HasIndex(u => u.Email)
            .IsUnique();
        
        builder.HasIndex(u => u.DisplayName);
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(u => u.DisplayName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(u => u.Bio)
            .HasMaxLength(500);
        
        builder.Property(u => u.CurrentMode)
            .HasDefaultValue(SocialMode.Leisure);
        
        builder.HasMany(u => u.Sessions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(u => u.Settings)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(u => u.Interests)
            .WithOne(ui => ui.User)
            .HasForeignKey(ui => ui.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}