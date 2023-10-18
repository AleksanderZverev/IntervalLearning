namespace IntervalLearningApi.IntegrationTests.Common.TestScopes;

public class ScopeApiTests : BaseApiTests, IClassFixture<DockerIntervalLearningApiFactory>, IAsyncLifetime
{
    public ScopeApiTests(DockerIntervalLearningApiFactory apiFactory) : base(apiFactory)
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