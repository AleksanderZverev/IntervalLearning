namespace GlobalTools.Extensions;

public static class DateTimeExtensions
{
    public static DateTime AddOffset(this DateTime dateTime, DateTimeOffset offset)
    {
        return dateTime.Add(offset.Offset);
    }
}