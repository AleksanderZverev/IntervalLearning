using DB.Models.Dictionary;
using DB.Models.Dictionary.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Dictionary;

public class WordTranslationConfiguration : IEntityTypeConfiguration<WordTranslation>
{
    public void Configure(EntityTypeBuilder<WordTranslation> builder)
    {
        builder.ToTable("Translations");
        
        builder.HasKey(t => new {t.WordId, t.LanguageId, t.Id});

        builder.Property(t => t.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(t => t.Translation)
            .HasMaxLength(255)
            .IsRequired()
            .HasConversion(
                d => d.Value,
                s => TranslationText.Create(s).Value);

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