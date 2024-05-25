using Domain.Schedule.ValueObjects;

namespace IntervalLearningApi.IntegrationTests.Learning.Scenarios;

public static partial class LearningScenarios
{
    public static List<ScenarioV2> ShouldMoveToStartWhenFeatureFlagEnabled =
        ScenarioV2.ScenariosFor(
            ForgottenBehavior.MoveToPreviousStep,
            ("3 days", new ScenarioStepV2[]
            {
                //1
                new(Weight.Remember, Move.Next),
                
                //3
                new(Weight.UnknownOrForgotten, Move.ToRepeating),
                new(Weight.Any, Move.ToStart),
            }),
            ("7 days", new ScenarioStepV2[]
            {
                //1
                new(Weight.Remember, Move.Next),
                
                //3
                new(Weight.Remember, Move.Next),
                
                //7
                new(Weight.UnknownOrForgotten, Move.ToRepeating),
                new(Weight.Any, Move.ToStart),
            }),
            ("14 days (forgotten)", new ScenarioStepV2[]
            {
                //1
                new(Weight.Remember, Move.Next),
                
                //3
                new(Weight.Remember, Move.Next),
                
                //7
                new(Weight.Remember, Move.Next),
                
                //14
                new(Weight.Forgotten, Move.ToRepeating),
                new(Weight.Any, Move.ToStart),
            }),
            ("14 days (unknown)", new ScenarioStepV2[]
            {
                //1
                new(Weight.Remember, Move.Next),
                
                //3
                new(Weight.Remember, Move.Next),
                
                //7
                new(Weight.Remember, Move.Next),
                
                //14
                new(Weight.Unknown, Move.ToRepeating),
                new(Weight.Remember, Move.Stay),
            }),
            ("28 days (forgotten)", new ScenarioStepV2[]
            {
                //1
                new(Weight.Remember, Move.Next),
                
                //3
                new(Weight.Remember, Move.Next),
                
                //7
                new(Weight.Remember, Move.Next),
                
                //14
                new(Weight.Remember, Move.Next),
                
                //28
                new(Weight.Forgotten, Move.ToRepeating), //Starting from 28 days duration should just move back
                new(Weight.Any, Move.Previous),
            }),
            ("28 days (unknown)", new ScenarioStepV2[]
            {
                //1
                new(Weight.Remember, Move.Next),
                
                //3
                new(Weight.Remember, Move.Next),
                
                //7
                new(Weight.Remember, Move.Next),
                
                //14
                new(Weight.Remember, Move.Next),
                
                //28
                new(Weight.Unknown, Move.ToRepeating), //Starting from 28 days duration should just move back
                new(Weight.Remember, Move.Stay),
            }),
            ("56 days (forgotten)", new ScenarioStepV2[]
            {
                //1
                new(Weight.Remember, Move.Next),
                
                //3
                new(Weight.Remember, Move.Next),
                
                //7
                new(Weight.Remember, Move.Next),
                
                //14
                new(Weight.Remember, Move.Next),
                
                //28
                new(Weight.Remember, Move.Next),
                
                //56
                new(Weight.Forgotten, Move.ToRepeating),
                new(Weight.Any, Move.Previous),
            }),
            ("56 days (unknown)", new ScenarioStepV2[]
            {
                //1
                new(Weight.Remember, Move.Next),
                
                //3
                new(Weight.Remember, Move.Next),
                
                //7
                new(Weight.Remember, Move.Next),
                
                //14
                new(Weight.Remember, Move.Next),
                
                //28
                new(Weight.Remember, Move.Next),
                
                //56
                new(Weight.Unknown, Move.ToRepeating),
                new(Weight.Remember, Move.Stay),
            }));
}