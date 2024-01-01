global using IntervalLearningApi.Services;
global using IntervalLearningApi.IntegrationTests.Learning;
global using Scenario = IntervalLearningApi.IntegrationTests.Learning.CardAndCollectionsControllerTests.Scenario;
global using ScenarioStep = IntervalLearningApi.IntegrationTests.Learning.CardAndCollectionsControllerTests.ScenarioStep;
using IntervalLearningApi.IntegrationTests.Learning.Common;

namespace IntervalLearningApi.IntegrationTests.Learning.Scenarios;

public static partial class LearningScenarios
{
    public const float RememberedWeight = 1f;
    public const float ForgottenWeight = 0f;
    public const float UnknownWeight = 0.5f;

    public static IEnumerable<object[]> ToMemberData(this List<Scenario> scenarios)
    {
        return scenarios.Select(s => new object[] { s });
    }
    
    private static List<ScenarioStep> stepsToTheLastStep = LearningCommons.phasesDuration
        .Select(_ => new ScenarioStep(RememberedWeight, 1))
        .Skip(1)
        .ToList();

    public static int DurationsCount = LearningCommons.phasesDuration.Count;
}