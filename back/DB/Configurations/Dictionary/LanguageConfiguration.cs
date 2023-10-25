using DB.Models.Dictionary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Dictionary;

public class LanguageConfiguration : IEntityTypeConfiguration<LanguageEntity>
{
    public void Configure(EntityTypeBuilder<LanguageEntity> builder)
    {
        builder.HasData(
            new LanguageEntity() {Id = 1, Name = "English", NativeLanguageName = "English"}, 
            new LanguageEntity() {Id = 2, Name = "Russian", NativeLanguageName = "Русский"},
            new LanguageEntity() {Id = 3, Name = "Japanese", NativeLanguageName = "日本語"});
    }
}