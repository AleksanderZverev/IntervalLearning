global using IntervalLearningApi.Models.ByUser;
global using IntervalLearningApi.Models.RepeatsSchedule;
global using IntervalLearningApi.Services;
global using DB.Models;
global using IntervalLearningApi.IntegrationTests.Learning;
global using Scenario = IntervalLearningApi.IntegrationTests.Learning.CardAndCollectionsControllerTests.Scenario;
global using ScenarioStep = IntervalLearningApi.IntegrationTests.Learning.CardAndCollectionsControllerTests.ScenarioStep;

namespace IntervalLearningApi.IntegrationTests.Learning.Scenarios;

public static partial class LearningScenarios
{
    private const float RememberedWeight = 1f;
    private const float ForgottenWeight = 0f;
    private const float UnknownWeight = 0.5f;

    public static IEnumerable<object[]> ToMemberData(this List<Scenario> scenarios)
    {
        return scenarios.Select(s => new object[] { s });
    }
}