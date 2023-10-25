using DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Study;

public class PhaseConfiguration : IEntityTypeConfiguration<PhaseEntity>
{
    public void Configure(EntityTypeBuilder<PhaseEntity> builder)
    {
        builder.HasKey(s => new { s.ParentUserId, s.ParentRepeatsScheduleId, s.Id });

        builder.ConfigureUserReference();

        builder.HasOne(p => p.ParentRepeatsSchedule)
            .WithMany(s => s.Phases)
            .HasForeignKey(p => new {p.ParentUserId, p.ParentRepeatsScheduleId})
            .OnDelete(DeleteBehavior.NoAction);
    }
}