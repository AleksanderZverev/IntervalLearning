using Domain.RelearningCard;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Study;

public class RelearnCardConfiguration : IEntityTypeConfiguration<RelearningCard>
{
    public void Configure(EntityTypeBuilder<RelearningCard> builder)
    {
        builder.ToTable("RelearningCards");

        builder.HasKey(p => new { p.UserId, p.CollectionId, p.CardId });

        builder.Property(p => p.UserId)
            .HasConversion(Converters.UserId);
        
        builder.Property(p => p.CollectionId)
            .HasConversion(Converters.CollectionId);
        
        builder.Property(p => p.CardId)
            .HasConversion(Converters.CardId);
    }
}