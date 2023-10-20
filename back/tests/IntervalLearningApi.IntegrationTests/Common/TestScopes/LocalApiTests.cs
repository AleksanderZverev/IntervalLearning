namespace IntervalLearningApi.IntegrationTests.Common.TestScopes;

public class LocalApiTests : BaseApiTests, IClassFixture<LocalIntervalLearningApiFactory>, IAsyncLifetime
{
    public LocalApiTests(LocalIntervalLearningApiFactory apiFactory) : base(apiFactory)
    {
    }
}