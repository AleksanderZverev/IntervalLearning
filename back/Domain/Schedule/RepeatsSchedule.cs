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


    private enum StepAnswers
    {
        Any = 0,
        Forgotten = 1,
        NotClear = 2,
        Remembered = 4,
    }

    private record Scenario(StepAnswers Answer, StepAnswers RepetitionAnswer)
    {
        // public Scenario(StepAnswers answer, StepAnswers repetitionAnswer)
        // {
        //     Answer = answer;
        //     RepetitionAnswer = repetitionAnswer;
        // }
        //
        // public StepAnswers Answer { get; }
        // public StepAnswers RepetitionAnswer { get; }
        //
        // public override string ToString()
        // {
        //     return $"{Answer}/{RepetitionAnswer}";
        // }
        //
        // public override int GetHashCode()
        // {
        //     return (int)Answer + (int)RepetitionAnswer;
        // }
    }

    //TODO: Check in test room retrieving scenarios
    // private static Dictionary<Scenario, Dictionary<Scenario, Func<int>>> Scenarios = new()
    // {
    //     {
    //         new(StepAnswers.Forgotten, StepAnswers.Any), new()
    //         {
    //             { new Scenario(StepAnswers.Forgotten, StepAnswers.Any), () => -2 },
    //             { new Scenario(StepAnswers.NotClear, StepAnswers.Any), () => 0 },
    //             { new Scenario(StepAnswers.Remembered, StepAnswers.Any), () => 1 },
    //         }
    //     },
    //     {
    //         new Scenario(StepAnswers.NotClear, StepAnswers.Any), new()
    //         {
    //             { new Scenario(StepAnswers.Forgotten, StepAnswers.Any), () => -1 },
    //             { new Scenario(StepAnswers.NotClear, StepAnswers.Any), () => -1 },
    //             { new Scenario(StepAnswers.Remembered, StepAnswers.Any), () => 1 },
    //         }
    //     },
    //     {
    //         new Scenario(StepAnswers.Any, StepAnswers.Any), new()
    //         {
    //             { new Scenario(StepAnswers.Forgotten, StepAnswers.Any), () => -1 },
    //             { new Scenario(StepAnswers.NotClear, StepAnswers.Any), () => 0 },
    //             { new Scenario(StepAnswers.Remembered, StepAnswers.Any), () => 1 },
    //         }
    //     }
    // };

    // private (int nextPhaseIndex, Phase? nextPhase) MoveNextStep(
    //     Remember currentRemember)
    // {
    //     var currentPhaseIndex = currentRemember.PhaseIndex;
    //     var currentPhase = GetPhase(currentPhaseIndex);
    //     
    //     var nextPhaseIndex = FindNextPhaseIndex(currentPhaseIndex);
    //     var nextPhase = FindPhase(nextPhaseIndex);
    // }

    private bool ShouldMoveOnRepeatingStep(Scenario current, Scenario previous)
    {
        return current.Answer is not StepAnswers.Remembered;
    }

    enum StepMovements
    {
        Back = -1,
        DoubleBack = -2,
        Forward = 1,
        Stay = 0,
        ToStart = -99,
    }
    

    private StepMovements GetNextStep(Scenario current, Scenario previous)
    {
        if (ForgottenBehavior is ForgottenBehavior.MoveToNextStep)
        {
            return StepMovements.Forward;
        }

        if (ForgottenBehavior is ForgottenBehavior.StartFromFirstStep)
        {
            return current.Answer is StepAnswers.Remembered
                ? StepMovements.Forward
                : current.Answer is StepAnswers.NotClear 
                ? StepMovements.Stay
                : StepMovements.ToStart;
        }

        if (ForgottenBehavior is ForgottenBehavior.StayOnCurrentStep)
        {
            return current.Answer is StepAnswers.Remembered
                ? StepMovements.Forward
                : StepMovements.Stay;
        }

        if (ForgottenBehavior is not ForgottenBehavior.MoveToPreviousStep)
        {
            throw new ArgumentOutOfRangeException("Unknown forgotten behaviour");
        } 
        
        if (current.Answer is StepAnswers.Remembered)
        {
            return StepMovements.Forward;
        }
        
        // NotClear + NotClear → step back
        // NotClear + Forgotten → step back
        // NotClear + (R)Forgotten → step back
        // NotClear + (R)(NotClear or Remembered) → stay
        if (current.Answer is StepAnswers.NotClear)
        {
            if (previous.Answer is StepAnswers.NotClear or StepAnswers.Forgotten)
            {
                return StepMovements.Back;
            }
            
            if (current.RepetitionAnswer is StepAnswers.Forgotten)
            {
                return StepMovements.Back;
            }
            
            return StepMovements.Stay;
        }

        // Forgotten + Forgotten → double step back
        // Forgotten + NotClear → step back
        // Forgotten + Remembered → step back
        if (current.Answer is StepAnswers.Forgotten)
        {
            if (previous.Answer is StepAnswers.Forgotten)
            {
                return StepMovements.DoubleBack;
            }
            
            return StepMovements.Back;
        }

        throw new ArgumentOutOfRangeException("Unknown answer");
    }

    private Phase? CalculateNextNotRepeatingPhase(StepMovements movements, Phase currentPhase)
    {
        if (movements is StepMovements.ToStart)
            return FindNotRepeatingPhaseOf(GetPhase(0).Id);

        if (movements is StepMovements.Forward)
            return FindNextNotRepeatingPhase(currentPhase.Id);
        
        var currentNonRepeatingPhase = FindNotRepeatingPhaseOf(currentPhase.Id);

        if (movements is StepMovements.Stay)
            return currentNonRepeatingPhase;
        
        var prevStep = FindPreviousNotRepeatingPhase(currentPhase.Id);

        if (prevStep == null)
            return currentNonRepeatingPhase;

        if (movements is StepMovements.Back)
            return prevStep;
        
        var prevPrevStep = FindPreviousNotRepeatingPhase(prevStep.Id);
        
        return prevPrevStep == null
            ? prevStep
            : prevPrevStep;
    }

    public Phase? GetNextPhase(Card.Card cardEntity)
    {
        var currentRemember = cardEntity.FindLastRemember() ?? throw new InvalidOperationException("Failure on searching last remember");
        var currentPhase = GetPhase(currentRemember.PhaseIndex);

        var currentNotRepeatingPhase = FindNotRepeatingPhaseOf(currentPhase.Id);
        var currentRepeatPhase = FindRepeatingPhaseOf(currentNotRepeatingPhase.Id);

        var previousRemember = cardEntity.FindPreviousByPhaseIndex(IndexOf(currentNotRepeatingPhase));
        var previousPhase = previousRemember != null ? FindPhase(previousRemember.PhaseIndex) : null;
        var previousNotRepeatingPhase = previousPhase != null ? FindNotRepeatingPhaseOf(previousPhase.Id) : null;
        var previousRepeatingPhase = previousNotRepeatingPhase != null ? FindRepeatingPhaseOf(previousNotRepeatingPhase.Id) : null;

        var currentDateRemember = cardEntity.FindRememberByPhaseIndex(IndexOf(currentNotRepeatingPhase));
        var currentDateRepeatingRemember = currentRepeatPhase != null ? cardEntity.FindRememberByPhaseIndex(IndexOf(currentRepeatPhase)) : null;
        var currentDateScenario = new Scenario(
            ToAnswers(currentDateRemember),
            ToAnswers(currentDateRepeatingRemember)
        );

        var prevDateScenario = new Scenario(
            ToAnswers(previousNotRepeatingPhase != null && previousNotRepeatingPhase.Id != currentNotRepeatingPhase.Id 
                ? cardEntity.FindRememberByPhaseIndex(IndexOf(previousNotRepeatingPhase)) 
                : null),
            ToAnswers(previousRepeatingPhase != null && previousRepeatingPhase.Id != currentRepeatPhase.Id 
                ? cardEntity.FindRememberByPhaseIndex(IndexOf(previousRepeatingPhase)) 
                : null)
        );

        var movement = GetNextStep(currentDateScenario, prevDateScenario);

        if (!currentPhase.IsRepeat() && currentRepeatPhase != null)
        {
            var shouldStepOnRepeatStep = ShouldMoveOnRepeatingStep(currentDateScenario, prevDateScenario);

            if (shouldStepOnRepeatStep)
                return currentRepeatPhase;
        }

        return CalculateNextNotRepeatingPhase(movement, currentNotRepeatingPhase);
    }

    private StepAnswers ToAnswers(Remember? currentDateRemember)
    {
        if (currentDateRemember == null)
            return StepAnswers.Any;

        if (currentDateRemember.IsRemembered())
            return StepAnswers.Remembered;

        if (currentDateRemember.IsNotClearRemember())
            return StepAnswers.NotClear;

        return StepAnswers.Forgotten;
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
    
    public Phase? FindNextPhase(PhaseId currentPhaseId)
    {
        return Phases
            .OrderBy(p => p.Id)
            .SkipWhile(p => p.Id != currentPhaseId)
            .SkipWhile(p => p.Id == currentPhaseId)
            .FirstOrDefault();
    }
    
    public Phase? FindNextNotRepeatingPhase(PhaseId currentPhaseId)
    {
        return Phases
            .OrderBy(p => p.Id)
            .SkipWhile(p => p.Id != currentPhaseId)
            .SkipWhile(p => p.Id == currentPhaseId)
            .FirstOrDefault(p => !p.IsRepeat());
    }
    
    public Phase? FindPreviousNotRepeatingPhase(PhaseId currentPhaseId)
    {
        return Phases
            .OrderBy(p => p.Id)
            .Reverse()
            .SkipWhile(p => p.Id != currentPhaseId)
            .SkipWhile(p => p.Id == currentPhaseId)
            .FirstOrDefault(p => !p.IsRepeat());
    }

    public Phase? FindNotRepeatingPhaseOf(PhaseId phaseId)
    {
        var phase = Phases.Single(p => p.Id == phaseId);

        if (!phase.IsRepeat())
            return phase;

        var phases = Phases.OrderBy(p => p.Id).ToList();
        var repeatPhaseIndex = phases.FindIndex(p => p.Id == phaseId);
        var phaseIndex = repeatPhaseIndex - 1;

        if (phaseIndex < 0 || phaseIndex >= phases.Count)
            return null;

        var phaseSupposedNotToBeRepeating = phases[phaseIndex];
        
        return !phaseSupposedNotToBeRepeating.IsRepeat() 
            ? phaseSupposedNotToBeRepeating 
            : null;
    }
    
    public Phase? FindRepeatingPhaseOf(PhaseId phaseId)
    {
        var phase = Phases.Single(p => p.Id == phaseId);

        if (phase.IsRepeat())
            return phase;

        var phases = Phases.OrderBy(p => p.Id).ToList();
        var repeatPhaseIndex = phases.FindIndex(p => p.Id == phaseId);
        var phaseIndex = repeatPhaseIndex + 1;

        if (phaseIndex < 0 || phaseIndex >= phases.Count)
            return null;

        var phaseSupposedToBeRepeating = phases[phaseIndex];
        return phaseSupposedToBeRepeating.IsRepeat() ? phaseSupposedToBeRepeating : null;
    }


    public int IndexOf(Phase phase)
    {
        return Phases
            .OrderBy(p => p.Id)
            .ToList()
            .FindIndex(p => p.Id == phase.Id);
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