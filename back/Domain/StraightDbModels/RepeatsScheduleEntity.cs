using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Card;
using Domain.User;
using Domain.User.ValueObjects;
using Infrastructure;

namespace DB.Models;

public interface IParentRepeatsScheduleReference : IParentUserReference
{
    public short ParentRepeatsScheduleId { get; set; }
    public RepeatsScheduleEntity? ParentRepeatsSchedule { get; set; }
}

public interface IPatchRepeatsSchedule
{
    public string Title { get; }
    public string? ShortDescription { get; }
    public string? OnStartLearningDescription { get; }
    public short CardsCountPerPhase { get; }
    public string? DefaultPhaseShortDescription { get; }
    public string? DefaultPhaseDescription { get; }
    public string? DefaultRepeatPhaseShortDescription { get; }
    public string? DefaultRepeatPhaseDescription { get; }
}

public interface ICreateRepeatsSchedule : IPatchRepeatsSchedule
{
    public UserId ParentUserId { get; }
    public ForgottenBehavior ForgottenBehavior { get; }
}

public class PatchRepeatsSchedule : IPatchRepeatsSchedule
{
    public string Title { get; }
    public string? ShortDescription { get;  }
    public string? OnStartLearningDescription { get;  }
    public short CardsCountPerPhase { get;  }
    public string? DefaultPhaseShortDescription { get;  }
    public string? DefaultPhaseDescription { get;  }
    public string? DefaultRepeatPhaseShortDescription { get;  }
    public string? DefaultRepeatPhaseDescription { get;  }

    public PatchRepeatsSchedule(
        short cardsCountPerPhase,
        string title,
        string? shortDescription,
        string? description,
        string? defaultPhaseShortDescription,
        string? defaultPhaseDescription,
        string? defaultRepeatPhaseShortDescription,
        string? defaultRepeatPhaseDescription)
    {
        CardsCountPerPhase = cardsCountPerPhase;
        Title = TextMaster.RemoveWhiteSpaces(title, true);
        ShortDescription = TextMaster.RemoveWhiteSpaces(shortDescription);
        OnStartLearningDescription = TextMaster.RemoveWhiteSpacesExceptNewLines(description);
        DefaultPhaseShortDescription = TextMaster.RemoveWhiteSpaces(defaultPhaseShortDescription);
        DefaultPhaseDescription = TextMaster.RemoveWhiteSpacesExceptNewLines(defaultPhaseDescription);
        DefaultRepeatPhaseShortDescription = TextMaster.RemoveWhiteSpaces(defaultRepeatPhaseShortDescription);
        DefaultRepeatPhaseDescription = TextMaster.RemoveWhiteSpacesExceptNewLines(defaultRepeatPhaseDescription);
    }
}

public class CreateScheduleItem : PatchRepeatsSchedule, ICreateRepeatsSchedule
{
    public UserId ParentUserId { get; set; }
    public ForgottenBehavior ForgottenBehavior { get; }

    public CreateScheduleItem(
        UserId parentUserId,
        short cardsCountPerPhase,
        ForgottenBehavior forgottenBehavior,
        string title,
        string? shortDescription,
        string? description,
        string? defaultPhaseShortDescription,
        string? defaultPhaseDescription,
        string? defaultRepeatPhaseShortDescription,
        string? defaultRepeatPhaseDescription)
        :
        base(
            cardsCountPerPhase,
            title,
            shortDescription,
            description,
            defaultPhaseShortDescription,
            defaultPhaseDescription,
            defaultRepeatPhaseShortDescription,
            defaultRepeatPhaseDescription)

    {
        ParentUserId = parentUserId;
        ForgottenBehavior = forgottenBehavior;
    }
}

[Table("RepeatsSchedules")]
public class RepeatsScheduleEntity : IParentUserReference, ICreateRepeatsSchedule
{
    private const int ShortDescriptionLength = 200;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public short Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; }

    [StringLength(ShortDescriptionLength)]
    public string? ShortDescription { get; set; }

    public string? OnStartLearningDescription { get; set; }

    [StringLength(ShortDescriptionLength)]
    public string? DefaultPhaseShortDescription { get; set; }
    public string? DefaultPhaseDescription { get; set; }

    [StringLength(ShortDescriptionLength)]
    public string? DefaultRepeatPhaseShortDescription { get; set; }
    public string? DefaultRepeatPhaseDescription { get; set; }

    [Required] 
    public short CardsCountPerPhase { get; set; }

    [Required]
    public ForgottenBehavior ForgottenBehavior { get; set; }

    [Required]
    [MaxLength(50)]
    public virtual List<PhaseEntity> Phases { get; set; }

    public bool IsArchived { get; set; }
    public bool IsRecommended { get; set; }

    public UserId ParentUserId { get; set; }
    public User? ParentUser { get; set; }

    public (int nextPhaseIndex, PhaseEntity? nextPhase) GetNextPhaseIndex(Card cardEntity, RememberEntity remember)
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

    public PhaseEntity? FindPhase(int phaseIndex)
    {
        if (phaseIndex < 0 || phaseIndex >= Phases.Count)
            return null;
        
        var sortedPhases = Phases.OrderBy(p => p.Id).ToList();
        return sortedPhases[phaseIndex];
    }

    public PhaseEntity GetPhase(int phaseIndex)
    {
        if (phaseIndex < 0 || phaseIndex >= Phases.Count)
            throw new ArgumentOutOfRangeException();
        
        var sortedPhases = Phases.OrderBy(p => p.Id).ToList();
        return sortedPhases[phaseIndex];
    }
}