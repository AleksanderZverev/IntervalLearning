using GlobalTools;

namespace Domain.UnitTests.Common.Mocks.DateTimeProvider;

public class DateTimeProviderMock : IDateTimeProvider
{
    public static DateTimeProviderMock Now { get; } = new();

    public DateTime UtcNow { get; }

    public DateTimeProviderMock()
    {
        this.UtcNow = DateTime.UtcNow;
    }

    public DateTimeProviderMock(DateTime utcNow)
    {
        this.UtcNow = utcNow;
    }
}