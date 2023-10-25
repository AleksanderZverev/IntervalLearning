using DB.Models.Dictionary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Dictionary;

public class TranslationConfiguration : IEntityTypeConfiguration<TranslationEntity>
{
    public void Configure(EntityTypeBuilder<TranslationEntity> builder)
    {
        builder.HasKey(t => new {t.WordId, t.LanguageId, t.Id});

        builder.HasOne(t => t.Word)
            .WithMany()
            .HasForeignKey(t => t.WordId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(t => t.Language)
            .WithMany()
            .HasForeignKey(t => t.LanguageId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}