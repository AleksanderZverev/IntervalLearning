using DB.Models.Store;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Store;

public class CollectionPublicationConfiguration : IEntityTypeConfiguration<CollectionPublicationEntity>
{
    public void Configure(EntityTypeBuilder<CollectionPublicationEntity> builder)
    {
        builder.HasKey(c => new {c.ParentUserId, c.ParentCollectionId});

        builder.HasOne(c => c.ParentUser)
            .WithMany()
            .HasForeignKey(c => c.ParentUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(c => c.Subscribers)
            .WithOne(s => s.CollectionPublication)
            .HasForeignKey(s => new {s.ParentUserId, s.ParentCollectionId})
            .OnDelete(DeleteBehavior.Cascade);
    }
}