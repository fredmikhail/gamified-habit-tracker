using HabitTracker.Api.Domain.Entities;
using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.Domain.ValueObjects;
using HabitTracker.Api.Services;

namespace HabitTracker.Tests.Services;

public sealed class StreakServiceTests
{
    [Fact]
    public void CalculateDailyStreak_WhenNoDatesAreCompleted_ReturnsZeroStreaks()
    {
        var service = new StreakService();

        var result =
            service.CalculateDailyStreak(
                new DateOnly(2026, 7, 20),
                new DateOnly(2026, 7, 22),
                []);

        Assert.Equal(0, result.CurrentStreak);
        Assert.Equal(0, result.LongestStreak);
    }

    [Fact]
    public void CalculateDailyStreak_WhenTodayIsIncomplete_PreservesStreakThroughYesterday()
    {
        var service = new StreakService();

        var result =
            service.CalculateDailyStreak(
                new DateOnly(2026, 7, 20),
                new DateOnly(2026, 7, 22),
                [
                    new DateOnly(2026, 7, 20),
                    new DateOnly(2026, 7, 21)
                ]);

        Assert.Equal(2, result.CurrentStreak);
        Assert.Equal(2, result.LongestStreak);
    }

    [Fact]
    public void CalculateDailyStreak_WhenPastDayWasMissed_ReturnsZeroCurrentStreak()
    {
        var service = new StreakService();

        var result =
            service.CalculateDailyStreak(
                new DateOnly(2026, 7, 18),
                new DateOnly(2026, 7, 22),
                [
                    new DateOnly(2026, 7, 18),
                new DateOnly(2026, 7, 19)
                ]);

        Assert.Equal(0, result.CurrentStreak);
        Assert.Equal(2, result.LongestStreak);
    }

    [Fact]
    public void CalculateDailyStreak_WhenCurrentRunIsShorter_PreservesLongestHistoricalRun()
    {
        var service = new StreakService();

        var result =
            service.CalculateDailyStreak(
                new DateOnly(2026, 7, 14),
                new DateOnly(2026, 7, 22),
                [
                    new DateOnly(2026, 7, 14),
                new DateOnly(2026, 7, 15),
                new DateOnly(2026, 7, 16),
                new DateOnly(2026, 7, 21),
                new DateOnly(2026, 7, 22)
                ]);

        Assert.Equal(2, result.CurrentStreak);
        Assert.Equal(3, result.LongestStreak);
    }

    [Fact]
    public void CalculateWeeklyStreak_WhenCurrentWeekIsIncomplete_PreservesCompletedWeeks()
    {
        var service = new StreakService();

        var result =
            service.CalculateWeeklyStreak(
                new DateOnly(2026, 7, 22),
                WeekStartDay.Monday,
                [
                    new WeeklyStreakTarget(
                    new DateOnly(2026, 7, 6),
                    null,
                    3)
                ],
                [
                    new DateOnly(2026, 7, 6),
                new DateOnly(2026, 7, 8),
                new DateOnly(2026, 7, 10),
                new DateOnly(2026, 7, 13),
                new DateOnly(2026, 7, 15),
                new DateOnly(2026, 7, 17),
                new DateOnly(2026, 7, 21)
                ]);

        Assert.Equal(2, result.CurrentStreak);
        Assert.Equal(2, result.LongestStreak);
    }

    [Fact]
    public void CalculateWeeklyStreak_WhenHabitStartsMidweek_IgnoresFailedPartialWeek()
    {
        var service = new StreakService();

        var result =
            service.CalculateWeeklyStreak(
                new DateOnly(2026, 7, 29),
                WeekStartDay.Monday,
                [
                    new WeeklyStreakTarget(
                    new DateOnly(2026, 7, 18),
                    null,
                    3)
                ],
                [
                    new DateOnly(2026, 7, 18),
                new DateOnly(2026, 7, 20),
                new DateOnly(2026, 7, 22),
                new DateOnly(2026, 7, 24),
                new DateOnly(2026, 7, 28)
                ]);

        Assert.Equal(1, result.CurrentStreak);
        Assert.Equal(1, result.LongestStreak);
    }

    [Fact]
    public void CalculateWeeklyStreak_WhenTargetChanges_UsesTargetEffectiveForEachWeek()
    {
        var service = new StreakService();

        var result =
            service.CalculateWeeklyStreak(
                new DateOnly(2026, 7, 29),
                WeekStartDay.Monday,
                [
                    new WeeklyStreakTarget(
                    new DateOnly(2026, 7, 6),
                    new DateOnly(2026, 7, 20),
                    2),
                new WeeklyStreakTarget(
                    new DateOnly(2026, 7, 20),
                    null,
                    3)
                ],
                [
                    new DateOnly(2026, 7, 6),
                new DateOnly(2026, 7, 8),
                new DateOnly(2026, 7, 13),
                new DateOnly(2026, 7, 15),
                new DateOnly(2026, 7, 20),
                new DateOnly(2026, 7, 22),
                new DateOnly(2026, 7, 24),
                new DateOnly(2026, 7, 28)
                ]);

        Assert.Equal(3, result.CurrentStreak);
        Assert.Equal(3, result.LongestStreak);
    }

    [Fact]
    public void CalculateHabitStreak_WhenDailyFrequencyResumes_StartsNewStreakSeries()
    {
        var service = new StreakService();
        var habitId = Guid.CreateVersion7();

        var configurations =
            new[]
            {
            new HabitConfigurationVersion
            {
                HabitId = habitId,
                FrequencyType = HabitFrequencyType.Daily,
                TargetCount = 1,
                EffectiveFromDate =
                    new DateOnly(2026, 7, 1),
                EffectiveToDateExclusive =
                    new DateOnly(2026, 7, 8)
            },
            new HabitConfigurationVersion
            {
                HabitId = habitId,
                FrequencyType = HabitFrequencyType.Weekly,
                TargetCount = 3,
                EffectiveFromDate =
                    new DateOnly(2026, 7, 8),
                EffectiveToDateExclusive =
                    new DateOnly(2026, 7, 15)
            },
            new HabitConfigurationVersion
            {
                HabitId = habitId,
                FrequencyType = HabitFrequencyType.Daily,
                TargetCount = 1,
                EffectiveFromDate =
                    new DateOnly(2026, 7, 15)
            }
            };

        var completions =
            new[]
            {
            CreateCompletion(
                habitId,
                new DateOnly(2026, 7, 1)),
            CreateCompletion(
                habitId,
                new DateOnly(2026, 7, 2)),
            CreateCompletion(
                habitId,
                new DateOnly(2026, 7, 3)),
            CreateCompletion(
                habitId,
                new DateOnly(2026, 7, 15)),
            CreateCompletion(
                habitId,
                new DateOnly(2026, 7, 16))
            };

        var result =
            service.CalculateHabitStreak(
                new DateOnly(2026, 7, 17),
                WeekStartDay.Monday,
                configurations,
                completions);

        Assert.Equal(2, result.CurrentStreak);
        Assert.Equal(2, result.LongestStreak);
    }

    [Fact]
    public void CalculateHabitStreak_WhenCompletionWasUndone_IgnoresCompletion()
    {
        var service = new StreakService();
        var habitId = Guid.CreateVersion7();

        var configurations =
            new[]
            {
            new HabitConfigurationVersion
            {
                HabitId = habitId,
                FrequencyType = HabitFrequencyType.Daily,
                TargetCount = 1,
                EffectiveFromDate =
                    new DateOnly(2026, 7, 20)
            }
            };

        var completions =
            new[]
            {
            CreateCompletion(
                habitId,
                new DateOnly(2026, 7, 20)),
            CreateCompletion(
                habitId,
                new DateOnly(2026, 7, 21),
                undone: true),
            CreateCompletion(
                habitId,
                new DateOnly(2026, 7, 22))
            };

        var result =
            service.CalculateHabitStreak(
                new DateOnly(2026, 7, 22),
                WeekStartDay.Monday,
                configurations,
                completions);

        Assert.Equal(1, result.CurrentStreak);
        Assert.Equal(1, result.LongestStreak);
    }

    private static HabitCompletion CreateCompletion(
    Guid habitId,
    DateOnly completedDate,
    bool undone = false)
    {
        return new HabitCompletion
        {
            HabitId = habitId,
            CompletedDate = completedDate,
            CompletedAtUtc =
                DateTime.SpecifyKind(
                    completedDate.ToDateTime(
                        new TimeOnly(12, 0)),
                    DateTimeKind.Utc),
            UndoneAtUtc =
                undone
                    ? DateTime.SpecifyKind(
                        completedDate.ToDateTime(
                            TimeOnly.MaxValue),
                        DateTimeKind.Utc)
                    : null
        };
    }
}
