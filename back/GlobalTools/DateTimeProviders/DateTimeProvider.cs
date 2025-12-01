namespace GlobalTools;

public interface IDateTimeProvider
{
    public DateTime UtcNow { get; }
}

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}


public static class DateTimeProviderExtensions
{
    public static DateTime UserNow(this IDateTimeProvider dateTimeProvider, DateTimeOffset userCurrentDateTime)
    {
        return dateTimeProvider.UtcNow + userCurrentDateTime.Offset;
    } 
}