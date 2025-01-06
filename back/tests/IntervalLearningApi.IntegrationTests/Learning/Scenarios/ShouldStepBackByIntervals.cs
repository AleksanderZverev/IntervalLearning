using Domain.Schedule.ValueObjects;

namespace IntervalLearningApi.IntegrationTests.Learning.Scenarios;

public static partial class LearningScenarios
{
    public static List<Scenario> ShouldStepBackByIntervals_DuplicatedDurations = new()
    {
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>()
        {
            //1 day
            new(RememberedWeight, 2),   //0
            new(RememberedWeight, 2),   //2
            //3 day
            new(RememberedWeight, 2),   //4
            new(RememberedWeight, 2),   //6 ← should step here on first forgotten behavior
            //7 day
            new(RememberedWeight, 2),   //8
            new(ForgottenWeight, 1),    //10
            new(RememberedWeight, -5),  //11 - 5 = 6
        }, ResultStep: 4),
        new Scenario(ForgottenBehavior.MoveToPreviousStep, new List<ScenarioStep>() //DOUBLE BACK
        {
            //1 day
            new(RememberedWeight, 2),   //0
            new(RememberedWeight, 2),   //2 ← should step here on second forgotten behavior
            
            //3 day
            new(RememberedWeight, 2),   //4
            new(RememberedWeight, 2),   //6 ← should step here on first forgotten behavior
            //7 day
            new(RememberedWeight, 2),   //8
            new(ForgottenWeight, 1),    //10
            new(RememberedWeight, -5),  //11 - 5 = 6
            //3 day
            new(ForgottenWeight, 1),    //6
            new(RememberedWeight, -5),  //7 - 5 = 2
        }, ResultStep: 2),
    };
}