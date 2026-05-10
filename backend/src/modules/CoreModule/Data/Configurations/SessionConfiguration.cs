using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AbujaSocialMetaverse.Modules.Core.Data.Entities;

namespace AbujaSocialMetaverse.Modules.Core.Data.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("core_sessions");
        
        builder.HasKey(s => s.Id);
        
        builder.HasIndex(s => s.Jti)
            .IsUnique();
        
        builder.HasIndex(s => s.RefreshToken)
            .IsUnique();
        
        builder.HasIndex(s => s.UserId);
        
        builder.Property(s => s.Jti)
            .IsRequired()
            .HasMaxLength(128);
        
        builder.Property(s => s.RefreshToken)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(s => s.UserAgent)
            .HasMaxLength(500);
        
        builder.Property(s => s.IpAddress)
            .HasMaxLength(45);
    }
}