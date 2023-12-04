using Domain.Deprecated.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Store;

public class PublicCollectionSubscriberConfiguration : IEntityTypeConfiguration<PublicCollectionSubscriber>
{
    public void Configure(EntityTypeBuilder<PublicCollectionSubscriber> builder)
    {
        builder.HasKey(s => new {s.ParentUserId, s.ParentCollectionId, s.SubscriberUserId});

        builder.HasOne(s => s.ParentUser)
            .WithMany()
            .HasForeignKey(s => s.ParentUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.ParentCollection)
            .WithMany()
            .HasForeignKey(c => new { c.ParentUserId, c.ParentCollectionId })
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(s => s.SubscriberUser)
            .WithMany()
            .HasForeignKey(s => s.SubscriberUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}