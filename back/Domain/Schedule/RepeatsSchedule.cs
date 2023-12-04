using System.ComponentModel.DataAnnotations;
using Domain.Common.ValueObjects.Text.MultiLine;
using Domain.Common.ValueObjects.Text.SingleLine;
using Domain.Schedule.Entities.Phase;
using Domain.Schedule.Entities.Remember;
using Domain.Schedule.ValueObjects;
using Domain.User;
using Domain.User.ValueObjects;

namespace Domain.Schedule;

public interface IParentRepeatsScheduleReference : IParentUserReference
{
    public ScheduleId ParentRepeatsScheduleId { get; set; }
    public RepeatsSchedule? ParentRepeatsSchedule { get; set; }
}

// [Table("RepeatsSchedules")]
public class RepeatsSchedule : AggregateRoot<ComplexScheduleId>, IParentUserReference
{
    // private const int ShortDescriptionLength = 200;

    // [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ScheduleId Id { get; set; }

    // [Required]
    // [StringLength(255)]
    public required ScheduleTitle Title { get; set; }

    // [StringLength(ShortDescriptionLength)]
    public LongSingleLineString? ShortDescription { get; set; }

    public LongMultiLineString? OnStartLearningDescription { get; set; }

    // [StringLength(ShortDescriptionLength)]
    public LongSingleLineString? DefaultPhaseShortDescription { get; set; }
    public LongMultiLineString? DefaultPhaseDescription { get; set; }

    // [StringLength(ShortDescriptionLength)]
    public LongSingleLineString? DefaultRepeatPhaseShortDescription { get; set; }
    public LongMultiLineString? DefaultRepeatPhaseDescription { get; set; }

    // [Required] 
    public required short CardsCountPerPhase { get; set; }
    // [Required]
    public required ForgottenBehavior ForgottenBehavior { get; set; }

    [Required]
    [MaxLength(50)]
    public virtual List<Phase> Phases { get; set; }

    public bool IsArchived { get; set; }
    public bool IsRecommended { get; set; }

    public UserId ParentUserId { get; set; }
    public virtual User.User? ParentUser { get; set; }

    public RepeatsSchedule(
        UserId parentUserId,
        ScheduleId id) 
        : base(new ComplexScheduleId
        {
            Id = id,
            ParentUserId = parentUserId,
        })
    {
        ParentUserId = parentUserId;
        Id = id;
    }

    public (int nextPhaseIndex, Phase? nextPhase) GetNextPhase(
        Card.Card cardEntity,
        Remember remember)
    {
        var currentPhaseIndex = remember.PhaseIndex;
        var currentPhase = GetPhase(currentPhaseIndex);
        
        var nextPhaseIndex = FindNextPhaseIndex(currentPhaseIndex);
        var nextPhase = FindPhase(nextPhaseIndex);

        if (nextPhase != null && nextPhase.IsRepeat())
        {
            if (remember.IsRemembered())
            {
                nextPhaseIndex++;
                nextPhase = FindPhase(nextPhaseIndex);
            }
            
            return (nextPhaseIndex, nextPhase);
        }

        if (currentPhase.IsRepeat() && currentPhaseIndex > 0)
        {
            var previousRemember = cardEntity.FindPreviousRemember(remember.Id);

            if (previousRemember != null)
            {
                currentPhaseIndex = previousRemember.PhaseIndex;
                currentPhase = FindPhase(currentPhaseIndex);
                remember = previousRemember;
            }
        }

        if (remember.IsRemembered())
        {
            return (nextPhaseIndex, nextPhase);
        }

        if (remember.IsNotClearRemember())
        {
            return ForgottenBehavior switch
            {
                ForgottenBehavior.MoveToNextStep => (nextPhaseIndex, nextPhase),
                ForgottenBehavior.MoveToPreviousStep => (currentPhaseIndex, currentPhase),
                ForgottenBehavior.StartFromFirstStep => (currentPhaseIndex, currentPhase),
                ForgottenBehavior.StayOnCurrentStep => (currentPhaseIndex, currentPhase),
                _ => throw new ArgumentOutOfRangeException("Unknown forgotten behaviour " + ForgottenBehavior),
            };
        }

        switch (ForgottenBehavior)
        {
            case ForgottenBehavior.MoveToNextStep:
                return (nextPhaseIndex, nextPhase);
            case ForgottenBehavior.StayOnCurrentStep:
                return (currentPhaseIndex, currentPhase);
            case ForgottenBehavior.StartFromFirstStep:
                return (0, GetPhase(0));
            case ForgottenBehavior.MoveToPreviousStep:
            {
                if (currentPhaseIndex == 0)
                    return (0, GetPhase(0));

                var previousPhaseIndex = currentPhaseIndex - 1;
                var previousPhase = GetPhase(previousPhaseIndex);

                var isPreviousPhaseRepeatPhase = previousPhase.IsRepeat();

                if (isPreviousPhaseRepeatPhase && previousPhaseIndex > 0)
                {
                    previousPhase = GetPhase(previousPhaseIndex - 1);
                    previousPhaseIndex -= 1;
                }
                
                return (previousPhaseIndex, previousPhase);
            }
            default: throw new ArgumentOutOfRangeException("Unknown forgotten behaviour " + ForgottenBehavior);
        };
    }

    private int FindNextPhaseIndex(int currentPhaseIndex)
    {
        return currentPhaseIndex + 1 < Phases.Count
            ? currentPhaseIndex + 1
            : -1;
    }

    public (Phase?, int) FindFirstPhase()
    {
        return (FindPhase(0), 0);
    }

    public Phase? FindPhase(int phaseIndex)
    {
        if (phaseIndex < 0 || phaseIndex >= Phases.Count)
            return null;
        
        var sortedPhases = Phases.OrderBy(p => p.Id).ToList();
        return sortedPhases[phaseIndex];
    }

    public Phase GetPhase(int phaseIndex)
    {
        if (phaseIndex < 0 || phaseIndex >= Phases.Count)
            throw new ArgumentOutOfRangeException();
        
        var sortedPhases = Phases.OrderBy(p => p.Id).ToList();
        return sortedPhases[phaseIndex];
    }
}