using DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Study;

public class PhaseRememberConfiguration : IEntityTypeConfiguration<PhaseRememberEntity>
{
    public void Configure(EntityTypeBuilder<PhaseRememberEntity> builder)
    {
        builder.HasKey(r => new {r.ParentUserId, r.ParentRepeatsScheduleId, r.ParentPhaseId, r.RepeatedUserId, r.Id});

        builder.ConfigureUserReference();

        builder.HasOne(p => p.RepeatedUser)
            .WithMany()
            .HasForeignKey(p => p.RepeatedUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(r => r.ParentRepeatsSchedule)
            .WithMany()
            .HasForeignKey(r => new {r.ParentUserId, r.ParentRepeatsScheduleId})
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(r => r.ParentPhase)
            .WithMany()
            .HasForeignKey(r => new { r.ParentUserId, r.ParentRepeatsScheduleId, r.ParentPhaseId })
            .OnDelete(DeleteBehavior.NoAction);
    }
}