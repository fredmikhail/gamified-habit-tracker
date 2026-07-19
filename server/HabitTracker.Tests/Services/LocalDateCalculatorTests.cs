using HabitTracker.Api.Services;

namespace HabitTracker.Tests.Services;

public sealed class LocalDateCalculatorTests
{
    [Fact]
    public void GetLocalDate_WhenTorontoIsStillOnPreviousEvening_ReturnsPreviousDate()
    {
        var utcNow = new DateTimeOffset(
            2026,
            7,
            20,
            2,
            30,
            0,
            TimeSpan.Zero);

        var result = LocalDateCalculator.GetLocalDate(
            utcNow,
            "America/Toronto");

        Assert.Equal(
            new DateOnly(2026, 7, 19),
            result);
    }

    [Fact]
    public void GetLocalDate_WhenTokyoIsOnCurrentMorning_ReturnsCurrentDate()
    {
        var utcNow = new DateTimeOffset(
            2026,
            7,
            20,
            2,
            30,
            0,
            TimeSpan.Zero);

        var result = LocalDateCalculator.GetLocalDate(
            utcNow,
            "Asia/Tokyo");

        Assert.Equal(
            new DateOnly(2026, 7, 20),
            result);
    }

    [Fact]
    public void GetLocalDate_WhenTorontoReachesMidnight_ReturnsNewDate()
    {
        var utcNow = new DateTimeOffset(
            2026,
            7,
            20,
            4,
            0,
            0,
            TimeSpan.Zero);

        var result = LocalDateCalculator.GetLocalDate(
            utcNow,
            "America/Toronto");

        Assert.Equal(
            new DateOnly(2026, 7, 20),
            result);
    }
}