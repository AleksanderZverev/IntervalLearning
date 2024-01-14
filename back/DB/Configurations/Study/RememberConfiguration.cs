using Domain.Card.ValueObjects;
using Domain.Schedule.Entities.Remember;
using Domain.Schedule.Entities.Remember.ValueObjects;
using Domain.Schedule.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Study;

public class RememberConfiguration: IEntityTypeConfiguration<Remember>
{
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

        builder.Property(r => r.Comment)
            .HasMaxLength(255)
            .HasConversion(Converters.MediumSingleLine.ToNullable());
        
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
