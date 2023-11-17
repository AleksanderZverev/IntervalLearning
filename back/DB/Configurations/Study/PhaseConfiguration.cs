using DB.Models;
using DB.Models.ValueObjects;
using Domain.User.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Study;

public class PhaseConfiguration : IEntityTypeConfiguration<Phase>
{
    public static string GetSequenceName(UserId parentUserId, ScheduleId scheduleId)
    {
        return $"phase_for_schedule_{scheduleId.Value}_of_user_{parentUserId.Value}";
    }

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
            .HasConversion(Converters.LongSingleLine.ToNullable());

        builder.Property(p => p.OnLearnDescription)
            .HasConversion(Converters.LongMultiLine.ToNullable());

        builder.ConfigureUserReference();

        builder.HasOne(p => p.ParentRepeatsSchedule)
            .WithMany(s => s.Phases)
            .HasForeignKey(p => new {p.ParentUserId, p.ParentRepeatsScheduleId})
            .OnDelete(DeleteBehavior.NoAction);
    }
}