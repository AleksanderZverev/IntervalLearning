using Domain.Dictionary.Word;
using Domain.Dictionary.Word.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Dictionary;

public class LanguageWordConfiguration : IEntityTypeConfiguration<LanguageWord>
{
    public void Configure(EntityTypeBuilder<LanguageWord> builder)
    {
        builder.ToTable("Words");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(w => w.Word)
            .HasMaxLength(255)
            .IsRequired()
            .HasConversion(
                d => d.Value,
                s => WordText.Create(s).Value);

        builder.Property(w => w.Pronunciation)
            .HasMaxLength(255)
            .HasConversion(
                d => d.Value,
                s => WordPronunciation.Create(s).Value);
        
        builder.HasOne(w => w.Language)
            .WithMany()
            .HasForeignKey(w => w.LanguageId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(w => w.Word);
    }
}