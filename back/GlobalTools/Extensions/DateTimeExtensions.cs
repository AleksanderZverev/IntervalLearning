namespace GlobalTools.Extensions;

public static class DateTimeExtensions
{
    public static DateTime AddOffset(this DateTime dateTime, DateTimeOffset offset)
    {
        return dateTime.Add(offset.Offset);
    }

    public static (DateTime From, DateTime To) GetDateRange(this DateTimeOffset dateTime)
    {
        var startOfDateInUtc = new DateTime(
                DateOnly.FromDateTime(dateTime.DateTime),
                TimeOnly.MinValue,
                DateTimeKind.Utc)
            .Add(-dateTime.Offset);
        var endOfDateInUtc = startOfDateInUtc.AddHours(24).AddMilliseconds(-1);
        return (startOfDateInUtc, endOfDateInUtc);
    }
}