using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace IntervalLearningApi.IntegrationTests.Common.Api;


public class LocalIntervalLearningApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
    
    public IServiceScope GetScope() 
        => Server.Services.CreateScope();
}