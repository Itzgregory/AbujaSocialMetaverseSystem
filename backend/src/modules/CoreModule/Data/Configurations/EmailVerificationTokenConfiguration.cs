using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AbujaSocialMetaverse.Modules.Core.Data.Entities;

namespace AbujaSocialMetaverse.Modules.Core.Data.Configurations;

public class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.ToTable("core_email_verification_tokens");
        
        builder.HasKey(t => t.Id);
        
        builder.HasIndex(t => t.Token)
            .IsUnique();
        
        builder.HasIndex(t => t.UserId);
        
        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.HasOne(t => t.User)
            .WithMany(u => u.EmailVerificationTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}