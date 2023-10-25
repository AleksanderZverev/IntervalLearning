using DB.Models;
using DB.Models.Store;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Study;

public class CollectionConfiguration : IEntityTypeConfiguration<CollectionEntity>
{
    public void Configure(EntityTypeBuilder<CollectionEntity> builder)
    {
        builder.HasKey(c => new {c.ParentUserId, c.Id});

        builder.HasOne(c => c.ParentUser)
            .WithMany(u => u.Collections)
            .HasForeignKey(c => c.ParentUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.Theme)
            .WithMany()
            .HasForeignKey(c => c.ThemeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.CollectionPublicationEntity)
            .WithOne(p => p.ParentCollection)
            .HasForeignKey<CollectionPublicationEntity>(c => new {c.ParentUserId, c.ParentCollectionId})
            .OnDelete(DeleteBehavior.Cascade);
    }
}