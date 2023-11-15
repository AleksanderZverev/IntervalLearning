using DB.Models;
using DB.Models.ValueObjects;
using Domain.Theme;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Study;

public class ThemeConfiguration : IEntityTypeConfiguration<Theme>
{
    public void Configure(EntityTypeBuilder<Theme> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(Converters.ThemeId);

        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired()
            .HasConversion(from => from.Value, title => ThemeTitle.Create(title).Value);

        builder.HasOne(t => t.Language)
            .WithMany()
            .HasForeignKey(t => t.LanguageId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}