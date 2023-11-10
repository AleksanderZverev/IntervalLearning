using DB.Models;
using DB.Models.Store;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Study;

public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder.ToTable("Collections");
        
        builder.HasKey(c => new {c.ParentUserId, c.Id});
        
        builder.HasOne(c => c.ParentUser)
            .WithMany() //u => u.Collections
            .HasForeignKey(c => c.ParentUserId)
            .OnDelete(DeleteBehavior.NoAction);

        // builder.OwnsOne(c => c.Id, b =>
        // {
        //     b.Property(p => p.UserId)
        //         .HasConversion(Converters.UserId)
        //         .HasColumnName("ParentUserId");
        //
        //     b.Property(p => p.Id)
        //         .HasColumnName("Id");
        // });

        builder.Property(c => c.Title)
            .HasMaxLength(100)
            .IsRequired()
            .HasConversion(s => s.Value, d => CollectionTitle.Create(d).Value);

        builder.HasOne(c => c.Theme)
            .WithMany()
            .HasForeignKey(c => c.ThemeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Property(c => c.CardsCount)
            .HasConversion(Converters.Counter);

        builder.Property(c => c.NotStartedCardsCount)
            .HasConversion(Converters.Counter);

        // builder.Property(c => c.Cards)
        //     .HasField("cards")
        //     .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(c => c.CollectionPublicationEntity)
            .WithOne(p => p.ParentCollection)
            .HasForeignKey<CollectionPublicationEntity>(c => new {c.ParentUserId, c.ParentCollectionId})
            .OnDelete(DeleteBehavior.Cascade);
    }
}