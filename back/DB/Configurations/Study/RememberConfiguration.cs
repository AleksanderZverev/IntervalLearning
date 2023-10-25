using DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Study;

public class RememberConfiguration: IEntityTypeConfiguration<RememberEntity>
{
    public void Configure(EntityTypeBuilder<RememberEntity> builder)
    {
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
