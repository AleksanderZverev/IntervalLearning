global using IntervalLearningApi.Services;
global using IntervalLearningApi.IntegrationTests.Learning;
global using Scenario = IntervalLearningApi.IntegrationTests.Learning.CardAndCollectionsControllerTests.Scenario;
global using ScenarioV2 = IntervalLearningApi.IntegrationTests.Learning.CardAndCollectionsControllerTests.ScenarioV2;
global using ScenarioStep = IntervalLearningApi.IntegrationTests.Learning.CardAndCollectionsControllerTests.ScenarioStep;
global using ScenarioStepV2 = IntervalLearningApi.IntegrationTests.Learning.CardAndCollectionsControllerTests.ScenarioStepV2;
using IntervalLearningApi.IntegrationTests.Learning.Common;

namespace IntervalLearningApi.IntegrationTests.Learning.Scenarios;

public static partial class LearningScenarios
{
    public const float RememberedWeight = 1f;
    public const float ForgottenWeight = 0f;
    public const float UnknownWeight = 0.5f;
    
    public enum Weight
    {
        Remember,
        Unknown,
        Forgotten,
        UnknownOrForgotten,
        Any,
    }
    
    public enum Move
    {
        Stay,
        Next,
        ToRepeating,
        Previous,
        ToStart
    }

    public static IEnumerable<object[]> ToMemberData(this List<Scenario> scenarios)
    {
        return scenarios.Select(s => new object[] { s });
    }
    
    public static IEnumerable<object[]> ToMemberData(this List<ScenarioV2> scenarios)
    {
        return scenarios.Select(s => new object[] { s });
    }
    
    private static List<ScenarioStep> stepsToTheLastStep = LearningCommons.phasesDuration
        .Select(_ => new ScenarioStep(RememberedWeight, 1))
        .Skip(1)
        .ToList();

    public static int DurationsCount = LearningCommons.phasesDuration.Count;
}