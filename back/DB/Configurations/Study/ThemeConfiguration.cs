using DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Study;

public class ThemeConfiguration : IEntityTypeConfiguration<ThemeEntity>
{
    public void Configure(EntityTypeBuilder<ThemeEntity> builder)
    {
        builder.HasOne(t => t.Language)
            .WithMany()
            .HasForeignKey(t => t.LanguageId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}