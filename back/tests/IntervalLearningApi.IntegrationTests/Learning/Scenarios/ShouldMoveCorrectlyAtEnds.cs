using Domain.Schedule.ValueObjects;

namespace IntervalLearningApi.IntegrationTests.Learning.Scenarios;

public static partial class LearningScenarios
{
    public static List<Scenario> ReachedEndScenarios = new()
    {
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>(stepsToTheLastStep)
        {
            new(ForgottenWeight, -1),
        }, ResultStep: DurationsCount - 1),
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>(stepsToTheLastStep)
        {
            new(UnknownWeight, 0),
        }, ResultStep: DurationsCount),
        
        
        new Scenario(ForgottenBehavior.StartFromFirstStep, new List<ScenarioStep>(stepsToTheLastStep)
        {
            new(ForgottenWeight, -99),
        }, ResultStep: 1),
        new Scenario(ForgottenBehavior.StartFromFirstStep, new List<ScenarioStep>(stepsToTheLastStep)
        {
            new(UnknownWeight, 0),
        }, ResultStep: DurationsCount),
        
        
        new Scenario(ForgottenBehavior.StayOnCurrentStep, new List<ScenarioStep>(stepsToTheLastStep)
        {
            new(ForgottenWeight, 0),
        }, ResultStep: DurationsCount),
        new Scenario(ForgottenBehavior.StayOnCurrentStep, new List<ScenarioStep>(stepsToTheLastStep)
        {
            new(UnknownWeight, 0),
        }, ResultStep: DurationsCount),
    };
}