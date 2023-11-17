using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DB.Models.ValueObjects;
using Domain;
using Domain.Schedule;
using Domain.User;
using Domain.User.ValueObjects;
using Infrastructure;

namespace DB.Models;

public interface IParentPhaseReference : IParentRepeatsScheduleReference
{
    public PhaseId ParentPhaseId { get; set; }
    public Phase ParentPhase { get; set; }
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
        Description = string.IsNullOrEmpty(description) 
            ? null 
            : TextMaster.RemoveWhiteSpaces(description);
        IsDefaultValueSide = isDefaultValueSide;
        ShortDescription = string.IsNullOrEmpty(description) 
            ? null 
            : TextMaster.RemoveWhiteSpaces(shortDescription);
    }
}

public class CreatePhaseItem : PatchPhaseItem
{
    public UserId ParentUserId { get; set; }
    public ScheduleId ParentRepeatsScheduleId { get; set; }
    public PhaseId Id { get; set; }
    public uint SecondsFromLastPhase { get; set; }

    public CreatePhaseItem(
        UserId parentUserId,
        PhaseId id,
        ScheduleId parentRepeatsScheduleId,
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
// [Table("SchedulePhases")]
public class Phase : Entity<ComplexPhaseId>, IParentRepeatsScheduleReference
{
    // public const int ShortDescriptionLength = 200;

    // [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public PhaseId Id { get; set; }

    // [Required]
    public required uint SecondsFromLastPhase { get; set; }

    // [StringLength(ShortDescriptionLength)]
    public LongSingleLineString? ShortDescription { get; set; }
    public LongMultiLineString? OnLearnDescription { get; set; }

    public bool IsDefaultValueSide { get; set; }

    public ScheduleId ParentRepeatsScheduleId { get; set; }
    public RepeatsSchedule? ParentRepeatsSchedule { get; set; }
    
    public UserId ParentUserId { get; set; }
    public User? ParentUser { get; set; }

    public Phase(ScheduleId parentRepeatsScheduleId, UserId parentUserId, PhaseId id) 
        : base(new ComplexPhaseId()
        {
            ParentUserId = parentUserId,
            ParentRepeatsScheduleId = parentRepeatsScheduleId,
            Id = id
        })
    {
        Id = id;
        ParentRepeatsScheduleId = parentRepeatsScheduleId;
        ParentUserId = parentUserId;
    }

    public bool IsRepeat()
    {
        return SecondsFromLastPhase < 10;
    }

    public DateTime GetNextDate(DateTime from)
        => from.AddSeconds(SecondsFromLastPhase);

    public TimeSpan GetDurationToNextPhase()
        => TimeSpan.FromSeconds(SecondsFromLastPhase);
}