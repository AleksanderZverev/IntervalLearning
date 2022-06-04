using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Infrastructure;

namespace DB.Models;

public interface IParentPhaseReference : IParentRepeatsScheduleReference
{
    public short ParentPhaseId { get; set; }
    public PhaseEntity ParentPhase { get; set; }
}

public class PatchPhaseItem
{
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public bool IsDefaultValueSide { get; set; }

    public PatchPhaseItem(
        string? shortDescription,
        string? description,
        bool isDefaultValueSide)
    {
        Description = TextMaster.RemoveWhiteSpaces(description);
        IsDefaultValueSide = isDefaultValueSide;
        ShortDescription = TextMaster.RemoveWhiteSpaces(shortDescription);
    }
}

public class CreatePhaseItem : PatchPhaseItem
{
    public long ParentUserId { get; set; }
    public short ParentRepeatsScheduleId { get; set; }
    public short Id { get; set; }
    public uint SecondsFromLastPhase { get; set; }

    public CreatePhaseItem(
        long parentUserId,
        short id,
        short parentRepeatsScheduleId,
        uint secondsFromLastPhase,
        string? shortDescription,
        string? description,
        bool isDefaultValueSide)
        :
        base(
            shortDescription,
            description,
            isDefaultValueSide)
    {
        ParentUserId = parentUserId;
        Id = id;
        ParentRepeatsScheduleId = parentRepeatsScheduleId;
        SecondsFromLastPhase = secondsFromLastPhase;
    }
}


//Первая фаза всегда = изучение на первом этапе
[Table("SchedulePhases")]
public class PhaseEntity : IParentRepeatsScheduleReference
{
    public const int ShortDescriptionLength = 200;

    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public short Id { get; set; }

    [Required]
    public uint SecondsFromLastPhase { get; set; }

    [StringLength(ShortDescriptionLength)]
    public string? ShortDescription { get; set; }
    [Column("OnLearnDescription")]
    public string? Description { get; set; }

    public bool IsDefaultValueSide { get; set; }

    public short ParentRepeatsScheduleId { get; set; }
    public RepeatsScheduleEntity? ParentRepeatsSchedule { get; set; }


    public long ParentUserId { get; set; }
    public UserEntity? ParentUser { get; set; }
}