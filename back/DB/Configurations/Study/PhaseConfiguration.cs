using Domain.Schedule.Entities.Phase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Study;

public class PhaseConfiguration : IEntityTypeConfiguration<Phase>
{
    public void Configure(EntityTypeBuilder<Phase> builder)
    {
        builder.ToTable("SchedulePhases");
        
        builder.HasKey(p => new { p.ParentUserId, p.ParentRepeatsScheduleId, p.Id });

        builder.Property(p => p.Id)
            .IsRequired()
            .HasConversion(Converters.PhaseId);

        builder.Property(p => p.SecondsFromLastPhase)
            .IsRequired();

        builder.Property(p => p.ShortDescription)
            .HasMaxLength(200)
            .HasConversion(Converters.LongSingleLine.ToEmptyString());

        builder.Property(p => p.OnLearnDescription)
            .HasConversion(Converters.LongMultiLine.ToEmptyString());

        builder.ConfigureUserReference();

        builder.HasOne(p => p.ParentRepeatsSchedule)
            .WithMany(s => s.Phases)
            .HasForeignKey(p => new {p.ParentUserId, p.ParentRepeatsScheduleId})
            .OnDelete(DeleteBehavior.NoAction);
    }
}