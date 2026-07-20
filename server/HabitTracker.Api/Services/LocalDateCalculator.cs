namespace HabitTracker.Api.Services;

public static class LocalDateCalculator
{
    public static DateOnly GetLocalDate(
        DateTimeOffset utcNow,
        string timeZoneId)
    {
        var timeZone =
            TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        var localNow =
            TimeZoneInfo.ConvertTime(utcNow, timeZone);

        return DateOnly.FromDateTime(localNow.DateTime);
    }
}
