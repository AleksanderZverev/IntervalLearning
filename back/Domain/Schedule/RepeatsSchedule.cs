using System.Diagnostics;
using System.Net.NetworkInformation;
using Domain.Common.ValueObjects.Text.MultiLine;
using Domain.Common.ValueObjects.Text.SingleLine;
using Domain.Schedule.Entities.Phase;
using Domain.Schedule.Entities.Phase.ValueObjects;
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

public class RepeatsSchedule : AggregateRoot<ComplexScheduleId>, IParentUserReference
{
    public ScheduleId Id { get; set; }
    public required ScheduleTitle Title { get; set; }
    public required short CardsCountPerPhase { get; set; }
    public required ForgottenBehavior ForgottenBehavior { get; set; }
    public virtual List<Phase> Phases { get; set; }

    public bool IsArchived { get; set; }
    public bool IsRecommended { get; set; }

    public UserId ParentUserId { get; set; }
    public virtual User.User? ParentUser { get; set; }
    
    public LongSingleLineString? ShortDescription { get; set; }
    public LongMultiLineString? OnStartLearningDescription { get; set; }
    
    public LongSingleLineString? DefaultPhaseShortDescription { get; set; }
    public LongMultiLineString? DefaultPhaseDescription { get; set; }
    
    public LongSingleLineString? DefaultRepeatPhaseShortDescription { get; set; }
    public LongMultiLineString? DefaultRepeatPhaseDescription { get; set; }
    
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

    private List<Phase>? _orderedPhases;
    private List<Phase> OrderedPhases => _orderedPhases ??= Phases.OrderBy(p => p.Id).ToList();


    private enum MemorizedDegree
    {
        Any = 0,
        Forgotten = 1,
        NotClear = 2,
        Remembered = 4,
    }
    
    enum PhaseMovement
    {
        Back = -1,
        DoubleBack = -2,
        Forward = 1,
        Stay = 0,
        ToStart = -99,
    }

    private record PhaseAnswers(MemorizedDegree MainAnswer, MemorizedDegree RepetitionAnswer);
    
    public Phase? GetNextPhase(Card.Card cardEntity)
    {
        var currentRemember = cardEntity.FindLastRemember() ?? throw new InvalidOperationException("Failure on searching last remember");
        var currentPhase = GetPhase(currentRemember.PhaseIndex);

        if (currentPhase.IsRepeat() && currentPhase.Id == GetFirstPhase().Id)
        {
            return CalculateNextNotRepeatingPhase(PhaseMovement.Forward, currentPhase);
        }

        var currentNotRepeatingPhase = FindNotRepeatingPhaseOf(currentPhase.Id);
        var currentRepeatPhase = FindRepeatingPhaseOf(currentNotRepeatingPhase.Id);

        var currentNotRepeatingRemember = cardEntity.FindRememberByPhaseIndex(IndexOf(currentNotRepeatingPhase));
        var currentRepeatingRemember = currentRepeatPhase != null ? cardEntity.FindRememberByPhaseIndex(IndexOf(currentRepeatPhase)) : null;
        var currentPhaseAnswers = new PhaseAnswers(
            MainAnswer: ToMemorizedDegree(currentNotRepeatingRemember),
            RepetitionAnswer: ToMemorizedDegree(currentRepeatingRemember)
        );

        var previousRemember = cardEntity.FindPreviousRememberByPhaseIndex(IndexOf(currentNotRepeatingPhase));
        var previousPhase = previousRemember != null ? FindPhase(previousRemember.PhaseIndex) : null;
        var previousNotRepeatingPhase = previousPhase != null ? FindNotRepeatingPhaseOf(previousPhase.Id) : null;
        var previousRepeatingPhase = previousNotRepeatingPhase != null ? FindRepeatingPhaseOf(previousNotRepeatingPhase.Id) : null;

        var previousPhaseAnswers = new PhaseAnswers(
            MainAnswer: ToMemorizedDegree(
                previousNotRepeatingPhase != null
                    ? cardEntity.FindRememberByPhaseIndex(IndexOf(previousNotRepeatingPhase))
                    : null),
            RepetitionAnswer: ToMemorizedDegree(
                previousRepeatingPhase != null
                    ? cardEntity.FindRememberByPhaseIndex(IndexOf(previousRepeatingPhase))
                    : null)
        );

        var movement = GetNextStep(currentPhaseAnswers, previousPhaseAnswers);

        if (!currentPhase.IsRepeat() && currentRepeatPhase != null)
        {
            var shouldStepOnRepeatStep = ShouldMoveOnRepeatingStep(currentPhaseAnswers, previousPhaseAnswers);

            if (shouldStepOnRepeatStep)
                return currentRepeatPhase;
        }

        return CalculateNextNotRepeatingPhase(movement, currentNotRepeatingPhase);
    }

    private Phase? CalculateNextNotRepeatingPhase(PhaseMovement movements, Phase currentPhase)
    {
        if (movements is PhaseMovement.ToStart)
            return FindNotRepeatingPhaseOf(GetFirstPhase().Id);

        if (movements is PhaseMovement.Forward)
            return FindNextNotRepeatingPhase(currentPhase.Id);
        
        var currentNonRepeatingPhase = FindNotRepeatingPhaseOf(currentPhase.Id);

        if (movements is PhaseMovement.Stay)
            return currentNonRepeatingPhase;
        
        var prevStep = FindPreviousNotRepeatingPhaseByDuration(currentPhase.Id);

        if (prevStep == null)
            return currentNonRepeatingPhase;

        if (movements is PhaseMovement.Back)
            return prevStep;

        if (movements is PhaseMovement.DoubleBack)
        {
            var prevPrevStep = FindPreviousNotRepeatingPhaseByDuration(prevStep.Id);

            return prevPrevStep == null
                ? prevStep
                : prevPrevStep;
        }

        Debug.Fail("Unknown movement");
        throw new ArgumentOutOfRangeException("Unknown movement");
    }

    private PhaseMovement GetNextStep(PhaseAnswers current, PhaseAnswers previous)
    {
        if (ForgottenBehavior is ForgottenBehavior.MoveToNextStep)
        {
            return PhaseMovement.Forward;
        }

        if (ForgottenBehavior is ForgottenBehavior.StartFromFirstStep)
        {
            return current.MainAnswer is MemorizedDegree.Remembered
                ? PhaseMovement.Forward
                : current.MainAnswer is MemorizedDegree.NotClear 
                    ? PhaseMovement.Stay
                    : PhaseMovement.ToStart;
        }

        if (ForgottenBehavior is ForgottenBehavior.StayOnCurrentStep)
        {
            return current.MainAnswer is MemorizedDegree.Remembered
                ? PhaseMovement.Forward
                : PhaseMovement.Stay;
        }

        if (ForgottenBehavior is not ForgottenBehavior.MoveToPreviousStep)
        {
            throw new ArgumentOutOfRangeException("Unknown forgotten behaviour");
        } 
        
        if (current.MainAnswer is MemorizedDegree.Remembered)
        {
            return PhaseMovement.Forward;
        }
        
        // NotClear + NotClear → step back
        // NotClear + Forgotten → step back
        // NotClear + (R)Forgotten → step back
        // NotClear + (R)(NotClear or Remembered) → stay
        if (current.MainAnswer is MemorizedDegree.NotClear)
        {
            if (previous.MainAnswer is MemorizedDegree.NotClear or MemorizedDegree.Forgotten)
            {
                return PhaseMovement.Back;
            }
            
            if (current.RepetitionAnswer is MemorizedDegree.Forgotten)
            {
                return PhaseMovement.Back;
            }
            
            return PhaseMovement.Stay;
        }

        // Forgotten + Forgotten → double step back
        // Forgotten + NotClear → step back
        // Forgotten + Remembered → step back
        if (current.MainAnswer is MemorizedDegree.Forgotten)
        {
            if (previous.MainAnswer is MemorizedDegree.Forgotten)
            {
                return PhaseMovement.DoubleBack;
            }
            
            return PhaseMovement.Back;
        }

        throw new ArgumentOutOfRangeException("Unknown answer");
    }

    private bool ShouldMoveOnRepeatingStep(PhaseAnswers current, PhaseAnswers previous)
    {
        return current.MainAnswer is not MemorizedDegree.Remembered;
    }

    private MemorizedDegree ToMemorizedDegree(Remember? currentDateRemember)
    {
        if (currentDateRemember == null)
            return MemorizedDegree.Any;

        if (currentDateRemember.IsRemembered())
            return MemorizedDegree.Remembered;

        if (currentDateRemember.IsNotClearRemember())
            return MemorizedDegree.NotClear;

        return MemorizedDegree.Forgotten;
    }

    public Phase GetFirstPhase()
    {
        return FindPhase(0) ?? throw new ArgumentOutOfRangeException("First phase is not found");
    }

    private Phase? FindNextNotRepeatingPhase(PhaseId currentPhaseId)
    {
        return OrderedPhases
            .SkipWhile(p => p.Id != currentPhaseId)
            .SkipWhile(p => p.Id == currentPhaseId)
            .FirstOrDefault(p => !p.IsRepeat());
    }

    private Phase? FindPreviousNotRepeatingPhaseByDuration(PhaseId currentPhaseId)
    {
        var phase = FindNotRepeatingPhaseOf(currentPhaseId);
        var phaseIndex = OrderedPhases.FindIndex(p => p.Id == phase.Id);

        for (var i = phaseIndex - 1; i >= 0; i--)
        {
            var targetPhase = OrderedPhases[i];
            
            if (targetPhase.IsRepeat())
                continue;

            var daysDiff = Math.Abs(targetPhase.GetDurationToNextPhase().TotalDays - phase.GetDurationToNextPhase().TotalDays);
            if (daysDiff >= 0.5)
                return targetPhase;
        }

        return null;
    }

    private Phase? FindNotRepeatingPhaseOf(PhaseId phaseId)
    {
        var unknownPhaseIndex = OrderedPhases.FindIndex(p => p.Id == phaseId);
        var unknownPhase = OrderedPhases[unknownPhaseIndex];

        if (!unknownPhase.IsRepeat())
            return unknownPhase;

        var phaseIndex = unknownPhaseIndex - 1;

        if (phaseIndex < 0 || phaseIndex >= OrderedPhases.Count)
            return null;

        var phaseSupposedNotToBeRepeating = OrderedPhases[phaseIndex];
        
        return !phaseSupposedNotToBeRepeating.IsRepeat() 
            ? phaseSupposedNotToBeRepeating 
            : null;
    }

    private Phase? FindRepeatingPhaseOf(PhaseId phaseId)
    {
        var unknownPhaseIndex = OrderedPhases.FindIndex(p => p.Id == phaseId);
        var unknownPhase = OrderedPhases[unknownPhaseIndex];

        if (unknownPhase.IsRepeat())
            return unknownPhase;

        var phaseIndex = unknownPhaseIndex + 1;

        if (phaseIndex < 0 || phaseIndex >= OrderedPhases.Count)
            return null;

        var phaseSupposedToBeRepeating = OrderedPhases[phaseIndex];
        return phaseSupposedToBeRepeating.IsRepeat() ? phaseSupposedToBeRepeating : null;
    }


    public int IndexOf(Phase phase)
    {
        return OrderedPhases.FindIndex(p => p.Id == phase.Id);
    }

    public Phase? FindPhase(int phaseIndex)
    {
        if (phaseIndex < 0 || phaseIndex >= Phases.Count)
            return null;
        
        return OrderedPhases[phaseIndex];
    }

    public Phase GetPhase(int phaseIndex)
    {
        if (phaseIndex < 0 || phaseIndex >= Phases.Count)
            throw new ArgumentOutOfRangeException();
        
        return OrderedPhases[phaseIndex];
    }
}