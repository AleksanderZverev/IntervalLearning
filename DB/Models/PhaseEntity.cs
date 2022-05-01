using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

public interface IParentPhaseReference : IParentRepeatsScheduleReference
{
    public short ParentPhaseId { get; set; }
    public PhaseEntity ParentPhase { get; set; }
}

//Первая фаза всегда = изучение на первом этапе
[Table("SchedulePhases")]
public class PhaseEntity : IParentRepeatsScheduleReference
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public short Id { get; set; }

    [Required]
    public uint SecondsFromLastPhase { get; set; }

    [StringLength(150)]
    public string? Description { get; set; }

    public short ParentRepeatsScheduleId { get; set; }
    public RepeatsScheduleEntity? ParentRepeatsSchedule { get; set; }


    public long ParentUserId { get; set; }
    public UserEntity? ParentUser { get; set; }

    public PhaseEntity(long parentUserId, short id, short parentRepeatsScheduleId, uint secondsFromLastPhase, string? description)
    {
        ParentUserId = parentUserId;
        Id = id;
        ParentRepeatsScheduleId = parentRepeatsScheduleId;
        SecondsFromLastPhase = secondsFromLastPhase;
        Description = description;
    }
}