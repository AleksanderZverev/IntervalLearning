using DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Study;

public class CardRepeatQueueConfiguration : IEntityTypeConfiguration<CardRepeatQueueEntity>
{
    public void Configure(EntityTypeBuilder<CardRepeatQueueEntity> builder)
    {
        builder.HasKey(q => new
        {
            q.ParentUserId,
            q.ParentCollectionId,
            q.ParentCardId,
            q.ParentRepeatsScheduleUserId,
            q.ParentRepeatsScheduleId,
            q.Id
        });

        builder.ConfigureUserReference();

        builder.HasOne(q => q.ParentCollection)
            .WithMany()
            .HasForeignKey(q => new { q.ParentUserId, q.ParentCollectionId})
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(q => q.ParentCard)
            .WithMany()
            .HasForeignKey(q => new { q.ParentUserId, q.ParentCollectionId, q.ParentCardId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(q => q.ParentRepeatsSchedule)
            .WithMany()
            .HasForeignKey(q => new {q.ParentRepeatsScheduleUserId, q.ParentRepeatsScheduleId})
            .OnDelete(DeleteBehavior.NoAction);
    }
}