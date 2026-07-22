using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.Domain.ValueObjects;

namespace HabitTracker.Api.Services;

public static class CalendarPeriodCalculator
{
    public static WeekPeriod GetWeekPeriod(
        DateOnly date,
        WeekStartDay weekStartsOn)
    {
        var configuredStartDay =
            ConvertToDayOfWeek(weekStartsOn);

        var daysSinceWeekStart =
            ((int)date.DayOfWeek
                - (int)configuredStartDay
                + 7)
            % 7;

        var startDate =
            date.AddDays(-daysSinceWeekStart);

        return new WeekPeriod(
            startDate,
            startDate.AddDays(6));
    }

    private static DayOfWeek ConvertToDayOfWeek(
        WeekStartDay weekStartsOn)
    {
        return weekStartsOn switch
        {
            WeekStartDay.Monday =>
                DayOfWeek.Monday,
            WeekStartDay.Tuesday =>
                DayOfWeek.Tuesday,
            WeekStartDay.Wednesday =>
                DayOfWeek.Wednesday,
            WeekStartDay.Thursday =>
                DayOfWeek.Thursday,
            WeekStartDay.Friday =>
                DayOfWeek.Friday,
            WeekStartDay.Saturday =>
                DayOfWeek.Saturday,
            WeekStartDay.Sunday =>
                DayOfWeek.Sunday,
            _ => throw new ArgumentOutOfRangeException(
                nameof(weekStartsOn),
                weekStartsOn,
                "Week start day must be a defined value.")
        };
    }
}
