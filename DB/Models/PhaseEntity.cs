using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

[Table("SchedulePhases")]
public class PhaseEntity : IParentRepeatsScheduleReference
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public byte Id { get; set; }

    [Required]
    public uint SecondsFromLastPhase { get; set; }

    [StringLength(150)]
    public string? Description { get; set; }

    public short ParentRepeatsScheduleId { get; set; }
    public RepeatsScheduleEntity? ParentRepeatsSchedule { get; set; }


    public long ParentUserId { get; set; }
    public UserEntity? ParentUser { get; set; }

    public PhaseEntity(long parentUserId, byte id, short parentRepeatsScheduleId, uint secondsFromLastPhase, string? description)
    {
        ParentUserId = parentUserId;
        Id = id;
        ParentRepeatsScheduleId = parentRepeatsScheduleId;
        SecondsFromLastPhase = secondsFromLastPhase;
        Description = description;
    }
}