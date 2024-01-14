using GlobalTools;

namespace IntervalLearningApi.IntegrationTests.Common.Services;

public class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => currentTime;
    
    private DateTime currentTime = DateTime.UtcNow;

    public void SetTime(DateTime dateTime)
    {
        currentTime = dateTime;
    }
}