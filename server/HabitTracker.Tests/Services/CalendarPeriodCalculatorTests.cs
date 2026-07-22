using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.Services;

namespace HabitTracker.Tests.Services;

public sealed class CalendarPeriodCalculatorTests
{
    public static TheoryData<
        DateOnly,
        WeekStartDay> SupportedWeekStarts =>
        new()
        {
            {
                new DateOnly(2026, 7, 20),
                WeekStartDay.Monday
            },
            {
                new DateOnly(2026, 7, 21),
                WeekStartDay.Tuesday
            },
            {
                new DateOnly(2026, 7, 22),
                WeekStartDay.Wednesday
            },
            {
                new DateOnly(2026, 7, 23),
                WeekStartDay.Thursday
            },
            {
                new DateOnly(2026, 7, 24),
                WeekStartDay.Friday
            },
            {
                new DateOnly(2026, 7, 25),
                WeekStartDay.Saturday
            },
            {
                new DateOnly(2026, 7, 26),
                WeekStartDay.Sunday
            }
        };

    [Theory]
    [MemberData(nameof(SupportedWeekStarts))]
    public void GetWeekPeriod_WhenDateIsConfiguredStart_ReturnsSevenDayPeriod(
        DateOnly date,
        WeekStartDay weekStartsOn)
    {
        var result =
            CalendarPeriodCalculator.GetWeekPeriod(
                date,
                weekStartsOn);

        Assert.Equal(
            date,
            result.StartDateInclusive);

        Assert.Equal(
            date.AddDays(6),
            result.EndDateInclusive);
    }

    [Theory]
    [InlineData(
        WeekStartDay.Monday,
        2026,
        7,
        20,
        2026,
        7,
        26)]
    [InlineData(
        WeekStartDay.Sunday,
        2026,
        7,
        19,
        2026,
        7,
        25)]
    [InlineData(
        WeekStartDay.Saturday,
        2026,
        7,
        18,
        2026,
        7,
        24)]
    public void GetWeekPeriod_WhenDateFallsInsideWeek_ReturnsConfiguredBoundaries(
        WeekStartDay weekStartsOn,
        int startYear,
        int startMonth,
        int startDay,
        int endYear,
        int endMonth,
        int endDay)
    {
        var date = new DateOnly(2026, 7, 22);

        var result =
            CalendarPeriodCalculator.GetWeekPeriod(
                date,
                weekStartsOn);

        Assert.Equal(
            new DateOnly(
                startYear,
                startMonth,
                startDay),
            result.StartDateInclusive);

        Assert.Equal(
            new DateOnly(
                endYear,
                endMonth,
                endDay),
            result.EndDateInclusive);
    }

    [Fact]
    public void GetWeekPeriod_WhenWeekCrossesYearBoundary_ReturnsDatesFromBothYears()
    {
        var date = new DateOnly(2027, 1, 1);

        var result =
            CalendarPeriodCalculator.GetWeekPeriod(
                date,
                WeekStartDay.Monday);

        Assert.Equal(
            new DateOnly(2026, 12, 28),
            result.StartDateInclusive);

        Assert.Equal(
            new DateOnly(2027, 1, 3),
            result.EndDateInclusive);
    }

    [Fact]
    public void GetWeekPeriod_WhenWeekStartIsUndefined_ThrowsArgumentOutOfRangeException()
    {
        var undefinedWeekStart =
            (WeekStartDay)999;

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                CalendarPeriodCalculator.GetWeekPeriod(
                    new DateOnly(2026, 7, 22),
                    undefinedWeekStart));
    }
}
