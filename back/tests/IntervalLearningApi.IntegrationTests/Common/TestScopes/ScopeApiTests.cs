namespace IntervalLearningApi.IntegrationTests.Common.TestScopes;

public class ScopeApiTests : BaseApiTests, IClassFixture<IntervalLearningApiFactory>, IAsyncLifetime
{
    public ScopeApiTests(IntervalLearningApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    public async Task InitializeAsync()
    {
        await base.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }
}