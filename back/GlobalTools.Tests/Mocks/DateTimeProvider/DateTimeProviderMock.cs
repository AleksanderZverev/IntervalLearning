using GlobalTools;

namespace Domain.UnitTests.Common.Mocks.DateTimeProvider;

public class DateTimeProviderMock : IDateTimeProvider
{
    public static DateTimeProviderMock Now { get; } = new();

    public DateTime UtcNow => setupDateTime + PassedTime;
    
    private DateTime creationTime = DateTime.UtcNow;
    private TimeSpan PassedTime => DateTime.UtcNow - creationTime;
    private DateTime setupDateTime; 

    public DateTimeProviderMock()
    {
        setupDateTime = DateTime.UtcNow;
    }

    public DateTimeProviderMock(DateTime utcNow)
    {
        setupDateTime = utcNow;
    }
}