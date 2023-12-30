using Domain.Schedule.ValueObjects;

namespace IntervalLearningApi.IntegrationTests.Learning.Scenarios;

public static partial class LearningScenarios
{
    public static List<Scenario> ShouldMoveAfterRepetitionCorreclty_IfForgotten = new()
    {
        new Scenario(ForgottenBehavior.MoveToNextStep, new List<ScenarioStep>()
        {
            new(UnknownWeight, 1),
            new(UnknownWeight, 1),
        }, ResultStep: 3),
        new Scenario(ForgottenBehavior.MoveToNextStep, new List<ScenarioStep>()
        {
            new(UnknownWeight, 1),
            new(ForgottenWeight, 1),
        }, ResultStep: 3),
        new Scenario(ForgottenBehavior.MoveToNextStep, new List<ScenarioStep>()
        {
            new(ForgottenWeight, 1),
            new(UnknownWeight, 1),
        }, ResultStep: 3),
        new Scenario(ForgottenBehavior.MoveToNextStep, new List<ScenarioStep>()
        {
            new(ForgottenWeight, 1),
            new(ForgottenWeight, 1),
        }, ResultStep: 3),
        
        
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>() //NotClear → (R)NotClear
        {
            new(RememberedWeight, 2),   //3
            new(UnknownWeight, 1),      //4
            new(UnknownWeight, -1),     //3
        }, ResultStep: 3),
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>() // NotClear → (R)Forgotten
        {
            new(RememberedWeight, 2),   //3
            new(UnknownWeight, 1),      //4
            new(ForgottenWeight, -3),   //3
        }, ResultStep: 3),
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>()  // Forgotten → (R)NotClear
        {
            new(RememberedWeight, 2),   //3
            new(ForgottenWeight, 1),    //4
            new(UnknownWeight, -3),     //1
        }, ResultStep: 1),
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>() // Forgotten → (R)Forgotten
        {
            new(RememberedWeight, 2),   //3
            new(ForgottenWeight, 1),    //4
            new(ForgottenWeight, -3),   //1
        }, ResultStep: 1),
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>() // Forgotten → Forgotten → DOUBLE BACK
        {
            new(RememberedWeight, 2),   //0
            
            new(RememberedWeight, 2),   //2 ← after second forgotten
            
            new(RememberedWeight, 2),   //4
            
            new(RememberedWeight, 2),   //6 ← after first forgotten
            
            new(ForgottenWeight, 1),    //8
            new(RememberedWeight, -3),  //9 - 3 → 6
            
            new(ForgottenWeight, 1),    //6
            new(RememberedWeight, -5),  //7 - 5 → 2
        }, ResultStep: 2),
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>() //NotClear → NotClear → BACK
        {
            new(RememberedWeight, 2),   //0
            
            new(RememberedWeight, 2),   //2 ← going on second unclear answer
            
            new(UnknownWeight, 1),      //4 ← staying here for the first time
            new(RememberedWeight, -1),  //5 - 1 = 4
            
            new(UnknownWeight, 1),      //4 
            new(RememberedWeight, -3),  //5 - 3 = 2
        }, ResultStep: 2),
        
        
        new Scenario(ForgottenBehavior.StartFromFirstStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 2),   //3
            new(RememberedWeight, 2),   //5
            new(UnknownWeight, 1),      //6
            new(UnknownWeight, -1),     //5
        }, ResultStep: 5),
        new Scenario(ForgottenBehavior.StartFromFirstStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 2),   //3
            new(RememberedWeight, 2),   //5
            new(UnknownWeight, 1),      //6
            new(ForgottenWeight, -1),   //5
        }, ResultStep: 5),
        new Scenario(ForgottenBehavior.StartFromFirstStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 2),   //3
            new(RememberedWeight, 2),   //5
            new(ForgottenWeight, 1),    //6
            new(UnknownWeight, -5),     //1
        }, ResultStep: 1),
        new Scenario(ForgottenBehavior.StartFromFirstStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 2),   //3
            new(RememberedWeight, 2),   //5
            new(ForgottenWeight, 1),    //6
            new(ForgottenWeight, -5),   //1
        }, ResultStep: 1),
        
        
        new Scenario(ForgottenBehavior.StayOnCurrentStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 2),   //3
            new(UnknownWeight, 1),      //4
            new(UnknownWeight, -1),     //3
        }, ResultStep: 3),
        new Scenario(ForgottenBehavior.StayOnCurrentStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 2),   //3
            new(UnknownWeight, 1),      //4
            new(UnknownWeight, -1),     //3
        }, ResultStep: 2),
        new Scenario(ForgottenBehavior.StayOnCurrentStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 2),   //3
            new(ForgottenWeight, 1),    //4
            new(ForgottenWeight, -1),   //3
        }, ResultStep: 3),
        new Scenario(ForgottenBehavior.StayOnCurrentStep, new List<ScenarioStep>()
        {
            new(RememberedWeight, 2),   //3
            new(ForgottenWeight, 1),    //4
            new(ForgottenWeight, -1),   //3
        }, ResultStep: 2),
    };
}