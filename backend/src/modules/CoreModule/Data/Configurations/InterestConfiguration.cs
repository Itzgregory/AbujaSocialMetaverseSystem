using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AbujaSocialMetaverse.Modules.Core.Data.Entities;

namespace AbujaSocialMetaverse.Modules.Core.Data.Configurations;

public class InterestConfiguration : IEntityTypeConfiguration<Interest>
{
    public void Configure(EntityTypeBuilder<Interest> builder)
    {
        builder.ToTable("core_interests");
        
        builder.HasKey(i => i.Id);
        
        builder.HasIndex(i => i.Name)
            .IsUnique();
        
        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(i => i.Category)
            .IsRequired()
            .HasMaxLength(50);
    }
}