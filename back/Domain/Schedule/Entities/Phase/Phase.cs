using Domain.Common.ValueObjects.Text.MultiLine;
using Domain.Common.ValueObjects.Text.SingleLine;
using Domain.Schedule.Entities.Phase.ValueObjects;
using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;
using GlobalTools;

namespace Domain.Schedule.Entities.Phase;

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
public class Phase : Entity<ComplexPhaseId>, IParentRepeatsScheduleReference
{
    public PhaseId Id { get; set; }
    public required uint SecondsFromLastPhase { get; set; }
    public bool IsDefaultValueSide { get; set; }
    
    public UserId ParentUserId { get; set; }
    public User.User? ParentUser { get; set; }

    public ScheduleId ParentRepeatsScheduleId { get; set; }
    public RepeatsSchedule? ParentRepeatsSchedule { get; set; }
    
    public LongSingleLineString? ShortDescription { get; set; }
    public LongMultiLineString? OnLearnDescription { get; set; }

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

    /// <summary>
    /// Checks if the phase is repeating forgotten words phase
    /// </summary>
    public bool IsRepeat()
    {
        return SecondsFromLastPhase < 10;
    }

    public DateTime GetNextDate(DateTime from)
        => from.AddSeconds(SecondsFromLastPhase);

    public TimeSpan GetDurationToNextPhase()
        => TimeSpan.FromSeconds(SecondsFromLastPhase);
}