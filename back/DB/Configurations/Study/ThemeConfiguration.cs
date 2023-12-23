using Domain.Theme;
using Domain.Theme.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Study;

public class ThemeConfiguration : IEntityTypeConfiguration<Theme>
{
    public static string GetSequenceName()
        => $"themes_for_collection"; 

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