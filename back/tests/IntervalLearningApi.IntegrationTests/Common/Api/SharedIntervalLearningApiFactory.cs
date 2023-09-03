namespace IntervalLearningApi.IntegrationTests.Common.Api;

public class SharedIntervalLearningApiFactory : IAsyncLifetime
{
    private static IntervalLearningApiFactory sharedApplicationFactory = new();
    private static int subscribers;
    private static bool initialized;
    private static SemaphoreSlim semaphore = new(1, 1);

    public IntervalLearningApiFactory SharedFactory => sharedApplicationFactory;

    public async Task InitializeAsync()
    {
        await semaphore.WaitAsync();
        
        try
        {
            subscribers++;
            
            if (initialized)
                return;

            await sharedApplicationFactory.InitializeAsync();
            initialized = true;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task DisposeAsync()
    {
        await semaphore.WaitAsync();

        try
        {
            subscribers--;

            if (subscribers <= 0)
            {
                await sharedApplicationFactory.DisposeAsync();
            }
        }
        finally
        {
            semaphore.Release();   
        }
    }
}