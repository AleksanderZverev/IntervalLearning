using DB.Models.Dictionary;
using Domain.Common.ValueObjects;
using Domain.Language;
using Domain.Language.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Dictionary;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.ToTable("Languages");
            
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasConversion(id => id.Value, id => LanguageId.Create(id).Value)
            .ValueGeneratedOnAdd();

        builder.Property(l => l.Name)
            .HasConversion(l => l.Value, value => ShortString.Create(value).Value)
            .HasMaxLength(50);
        
        builder.Property(l => l.NativeLanguageName)
            .HasConversion(l => l.Value, value => ShortString.Create(value).Value)
            .HasMaxLength(50);
        
        builder.Property(l => l.TranslationLinkTitle)
            .HasConversion(l => l.Value, value => ShortString.Create(value).Value)
            .HasMaxLength(50);

        builder.HasData(
            Language.Create(1, "English", "English").Value,
            Language.Create(2, "Russian", "Русский").Value,
            Language.Create(3, "Japanese", "日本語").Value);
    }
}