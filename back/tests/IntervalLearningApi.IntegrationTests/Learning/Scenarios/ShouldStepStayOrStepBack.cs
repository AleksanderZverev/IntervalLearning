using Domain.Schedule.ValueObjects;

namespace IntervalLearningApi.IntegrationTests.Learning.Scenarios;

public static partial class LearningScenarios
{
    public static List<Scenario> ShouldStepStayOrStepBackScenarios = new()
    {
        new Scenario(ForgottenBehavior.MoveToNextStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 1),
            new(UnknownWeight, 1),
            new(ForgottenWeight, 1),
        }, ResultStep: 4),
        
        
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 1),
            new(RememberedWeight, 1),
            new(ForgottenWeight, -1),
        }, ResultStep: 2),
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 1),
            new(RememberedWeight, 1),
            new(UnknownWeight, 0),
        }, ResultStep: 3),
        
        
        new Scenario(ForgottenBehavior.StartFromFirstStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 1),
            new(RememberedWeight, 1),
            new(ForgottenWeight, -99),
        }, ResultStep: 1),
        new Scenario(ForgottenBehavior.StartFromFirstStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 1),
            new(RememberedWeight, 1),
            new(UnknownWeight, 0),
        }, ResultStep: 3),
        
        
        new Scenario(ForgottenBehavior.StayOnCurrentStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 1),
            new(RememberedWeight, 1),
            new(ForgottenWeight, 0),
        }, ResultStep: 3),
        new Scenario(ForgottenBehavior.StayOnCurrentStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 1),
            new(RememberedWeight, 1),
            new(UnknownWeight, 0),
        }, ResultStep: 3),
    };
}