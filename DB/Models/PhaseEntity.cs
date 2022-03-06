using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

[Table("SchedulePhases")]
public class PhaseEntity : IParentRepeatsScheduleReference
{
    [Key]
    public byte Id { get; set; }

    [Required]
    public uint SecondsFromLastPhase { get; set; }

    [StringLength(150)]
    public string? Description { get; set; }

    public short ParentRepeatsScheduleId { get; set; }
    public RepeatsScheduleEntity? ParentRepeatsSchedule { get; set; }


    public long ParentUserId { get; set; }
    public UserEntity? ParentUser { get; set; }
}