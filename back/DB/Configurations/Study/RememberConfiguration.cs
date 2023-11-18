using DB.Models;
using DB.Models.ValueObjects;
using Domain.Card.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Study;

public class RememberConfiguration: IEntityTypeConfiguration<Remember>
{
    public static string GetSequenceName(ComplexScheduleId schedule, ComplexCardId card)
    {
        return $"remember_" +
               $"schedule_{schedule.ParentUserId}_{schedule.Id}_" +
               $"card_{card.UserId}_{card.CollectionId}_{card.Id}";
    }

    public void Configure(EntityTypeBuilder<Remember> builder)
    {
        builder.ToTable("RememberWeights");
        
        builder.HasKey(r => new
        {
            r.ParentUserId,
            r.ParentCollectionId,
            r.ParentCardId,
            r.ParentRepeatsScheduleUserId,
            r.ParentRepeatsScheduleId,
            r.PhaseIndex,
            r.Id
        });

        builder.Property(r => r.Id)
            .ValueGeneratedOnAdd()
            .IsRequired()
            .HasConversion(
                d => d.Value,
                s => RememberId.Create(s).Value);

        builder.Property(r => r.Weight)
            .IsRequired()
            .HasConversion(
                d => d.Value,
                s => RememberWeight.Create(s).Value);
        
        builder.ConfigureUserReference();

        builder.HasOne(r => r.ParentCollection)
            .WithMany()
            .HasForeignKey(e => new { e.ParentUserId, e.ParentCollectionId })
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(r => r.ParentCard)
            .WithMany(r => r.Remembers)
            .HasForeignKey(c => new { c.ParentUserId, c.ParentCollectionId, c.ParentCardId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.ParentRepeatsSchedule)
            .WithMany()
            .HasForeignKey(c => new {c.ParentRepeatsScheduleUserId, c.ParentRepeatsScheduleId})
            .OnDelete(DeleteBehavior.NoAction);
    }
}
