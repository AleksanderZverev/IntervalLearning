using Domain.Schedule;
using Domain.Schedule.ValueObjects;

namespace IntervalLearningApi.IntegrationTests.Learning.Scenarios;

public static partial class LearningScenarios
{
    public static List<Scenario> ShouldStepOnRepetitionScenarios = new()
    {
        new Scenario(ForgottenBehavior.MoveToNextStep, new List<ScenarioStep>()
        {
            new( RememberedWeight, 2), //skipping repetition +1
        }, ResultStep: 3),
        new Scenario(ForgottenBehavior.MoveToNextStep, new List<ScenarioStep>()
        {
            new(UnknownWeight, 1),
        }, ResultStep: 2),
        new Scenario(ForgottenBehavior.MoveToNextStep, new List<ScenarioStep>()
        {
            new(ForgottenWeight, 1),
        }, ResultStep: 2),
        
        
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 2), //skipping repetition +1
        }, ResultStep: 3),
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>()
        {
            new(UnknownWeight, 1),
        }, ResultStep: 2),
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>()
        {
            new(ForgottenWeight, 1),
        }, ResultStep: 2),
        
        
        new Scenario(ForgottenBehavior.StartFromFirstStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 2), //skipping repetition +1
        }, ResultStep: 3),
        new Scenario(ForgottenBehavior.StartFromFirstStep, new List<ScenarioStep>()
        {
            new(UnknownWeight, 1),
        }, ResultStep: 2),
        new Scenario(ForgottenBehavior.StartFromFirstStep, new List<ScenarioStep>()
        {
            new(ForgottenWeight, 1),
        }, ResultStep: 2),
        
        
        new Scenario(ForgottenBehavior.StayOnCurrentStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 2), //skipping repetition +1
        }, ResultStep: 3),
        new Scenario(ForgottenBehavior.StayOnCurrentStep, new List<ScenarioStep>()
        {
            new(UnknownWeight, 1),
        }, ResultStep: 2),
        new Scenario(ForgottenBehavior.StayOnCurrentStep, new List<ScenarioStep>()
        {
            new(ForgottenWeight, 1),
        }, ResultStep: 2),
    };
}