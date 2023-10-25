using DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Study;

public class RepeatScheduleConfiguration : IEntityTypeConfiguration<RepeatsScheduleEntity>
{
    public void Configure(EntityTypeBuilder<RepeatsScheduleEntity> builder)
    {
        builder.HasKey(s => new {s.ParentUserId, s.Id});
        builder.ConfigureUserReference();
    }
}