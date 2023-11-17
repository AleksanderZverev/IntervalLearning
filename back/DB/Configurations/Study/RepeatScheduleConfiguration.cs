using DB.Models;
using DB.Models.ValueObjects;
using Domain.Schedule;
using Domain.User.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DB.Configurations.Study;

public class RepeatScheduleConfiguration : IEntityTypeConfiguration<RepeatsSchedule>
{
    public static string GetSequenceName(UserId userId)
        => $"schedule_of_user_{userId.Value}";
    
    public void Configure(EntityTypeBuilder<RepeatsSchedule> builder)
    {
        builder.ToTable("RepeatsSchedules");
        
        builder.HasKey(s => new {s.ParentUserId, s.Id});

        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd()
            .HasConversion(Converters.ScheduleId);

        builder.Property(s => s.Title)
            .HasMaxLength(255)
            .IsRequired()
            .HasConversion(d => d.Value, s => ScheduleTitle.Create(s).Value);

        builder.HasOne(s => s.ParentUser)
            .WithMany()
            .HasForeignKey(s => s.ParentUserId)
            .OnDelete(DeleteBehavior.NoAction);

        //short
        builder.Property(s => s.ShortDescription)
            .HasMaxLength(200)
            .HasConversion(Converters.ScheduleShortDescription.ToNullable());

        builder.Property(s => s.DefaultPhaseShortDescription)
            .HasMaxLength(200)
            .HasConversion(Converters.ScheduleShortDescription.ToNullable());
        
        builder.Property(s => s.DefaultRepeatPhaseShortDescription)
            .HasMaxLength(200)
            .HasConversion(Converters.ScheduleShortDescription.ToNullable());
        
        //long
        builder.Property(s => s.OnStartLearningDescription)
            .HasConversion(Converters.ScheduleLongDescription.ToNullable());
        
        builder.Property(s => s.DefaultPhaseDescription)
            .HasConversion(Converters.ScheduleLongDescription.ToNullable());
        
        builder.Property(s => s.DefaultRepeatPhaseDescription)
            .HasConversion(Converters.ScheduleLongDescription.ToNullable());
    }
}