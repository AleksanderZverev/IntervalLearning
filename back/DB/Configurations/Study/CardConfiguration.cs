using DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Study;

public class CardConfiguration  : IEntityTypeConfiguration<CardEntity>
{
    public void Configure(EntityTypeBuilder<CardEntity> builder)
    {
        builder.HasKey(c => new {c.ParentUserId, c.ParentCollectionId, c.Id});
            
        builder.HasOne(c => c.ParentUser)
            .WithMany()
            .HasForeignKey(c => c.ParentUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.ParentCollection)
            .WithMany() //c => c.Cards
            .HasForeignKey(c => new {c.ParentUserId, c.ParentCollectionId})
            .OnDelete(DeleteBehavior.NoAction);
    }
}