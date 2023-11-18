using DB.Models;
using DB.Models.ValueObjects;
using Domain.Card;
using Domain.Schedule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Study;

public class CardRepeatQueueConfiguration : IEntityTypeConfiguration<CardRepeatQueue>
{
    public static string GetSequenceName(RepeatsSchedule scheduleWithPhases, Card card)
    {
        return $"queue_" +
               $"schedule_{scheduleWithPhases.ParentUserId.Value}_{scheduleWithPhases.Id}_" +
               $"card_{card.ParentUserId.Value}_{card.ParentCollectionId.Value}_{card.Id.Value}";
    }

    public void Configure(EntityTypeBuilder<CardRepeatQueue> builder)
    {
        builder.ToTable("Queue");
        
        builder.HasKey(q => new
        {
            q.ParentUserId,
            q.ParentCollectionId,
            q.ParentCardId,
            q.ParentRepeatsScheduleUserId,
            q.ParentRepeatsScheduleId,
            q.Id
        });

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(d => d.Value, s => QueueId.Create(s).Value)
            .IsRequired();

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