using DB.Models.Dictionary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Dictionary;

public class WordConfiguration : IEntityTypeConfiguration<WordEntity>
{
    public void Configure(EntityTypeBuilder<WordEntity> builder)
    {
        builder.HasOne(w => w.Language)
            .WithMany()
            .HasForeignKey(w => w.LanguageId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(w => w.Word);
    }
}