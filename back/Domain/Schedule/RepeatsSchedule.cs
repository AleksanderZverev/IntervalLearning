using System.ComponentModel.DataAnnotations;
using DB.Models;
using DB.Models.ValueObjects;
using Domain.Schedule.ValueObjects;
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

    public (Phase?, int) GetNextPhase(Card.Card card)
    {
        var lastRemember = card.FindLastRemember();

        if (lastRemember == null)
            return (null, 0);

        var nextPhaseIndex = lastRemember.PhaseIndex + 1;
        var nextPhase = FindPhase(nextPhaseIndex);
        return (nextPhase, nextPhaseIndex);
    }

    public (int nextPhaseIndex, Phase? nextPhase) GetNextPhase(
        Card.Card cardEntity,
        RememberEntity remember)
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
            var previousRemember = cardEntity.FindLastRemember();

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