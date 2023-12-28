using Domain.Schedule.ValueObjects;

namespace IntervalLearningApi.IntegrationTests.Learning.Scenarios;

public static partial class LearningScenarios
{
    public static List<Scenario> TestOnTheStartScenarios = new()
    {
        new Scenario(ForgottenBehavior.MoveToNextStep, new List<ScenarioStep>()
        {
            new(ForgottenWeight, 1),
        }, ResultStep: 2),
        new Scenario(ForgottenBehavior.MoveToNextStep, new List<ScenarioStep>()
        {
            new(UnknownWeight, 1),
        }, ResultStep: 2),
        
        
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>()
        {
            new(ForgottenWeight, -1),
        }, ResultStep: 1),
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>()
        {
            new(UnknownWeight, 0),
        }, ResultStep: 1),
        
        
        new Scenario(ForgottenBehavior.StartFromFirstStep, new List<ScenarioStep>()
        {
            new(ForgottenWeight, -99),
        }, ResultStep: 1),
        new Scenario(ForgottenBehavior.StartFromFirstStep, new List<ScenarioStep>()
        {
            new(UnknownWeight, 0),
        }, ResultStep: 1),
        
        
        new Scenario(ForgottenBehavior.StayOnCurrentStep, new List<ScenarioStep>()
        {
            new(ForgottenWeight, 0),
        }, ResultStep: 1),
        new Scenario(ForgottenBehavior.StayOnCurrentStep, new List<ScenarioStep>()
        {
            new(UnknownWeight, 0),
        }, ResultStep: 1),
    };
}