using Domain.Schedule.ValueObjects;

namespace IntervalLearningApi.IntegrationTests.Learning.Scenarios;

public static partial class LearningScenarios
{
    public static List<Scenario> OnCompletingScenarios = new()
    {
        new Scenario(ForgottenBehavior.MoveToNextStep, new List<ScenarioStep>(stepsToTheLastStep)
        {
            new(RememberedWeight, 1),
        }, ResultStep: DurationsCount + 1),
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>(stepsToTheLastStep)
        {
            new(RememberedWeight, 1),
        }, ResultStep: DurationsCount + 1),
        new Scenario(ForgottenBehavior.StartFromFirstStep, new List<ScenarioStep>(stepsToTheLastStep)
        {
            new(RememberedWeight, 1),
        }, ResultStep: DurationsCount + 1),
        new Scenario(ForgottenBehavior.StayOnCurrentStep, new List<ScenarioStep>(stepsToTheLastStep)
        {
            new(RememberedWeight, 1),
        }, ResultStep: DurationsCount + 1),
    };
}