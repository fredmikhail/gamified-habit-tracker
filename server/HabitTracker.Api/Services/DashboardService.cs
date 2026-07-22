using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Services;

public sealed class DashboardService
{
    private readonly AppDbContext _dbContext;
    private readonly XpService _xpService;
    private readonly StreakService _streakService;
    private readonly TimeProvider _timeProvider;

    public DashboardService(
        AppDbContext dbContext,
        XpService xpService,
        StreakService streakService,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _xpService = xpService;
        _streakService = streakService;
        _timeProvider = timeProvider;
    }

    public async Task<DashboardResponse> GetDashboardAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var settings =
            await _dbContext.UserSettings
                .Where(settings =>
                    settings.UserId == userId)
                .Select(settings =>
                    new
                    {
                        settings.TimeZone,
                        settings.WeekStartsOn
                    })
                .SingleAsync(cancellationToken);

        var localDate =
            LocalDateCalculator.GetLocalDate(
                _timeProvider.GetUtcNow(),
                settings.TimeZone);

        var totalXp =
            await _dbContext.XpTransactions
                .Where(transaction =>
                    transaction.UserId == userId)
                .SumAsync(
                    transaction => transaction.Amount,
                    cancellationToken);

        var overallProgress =
            _xpService.CalculateOverallLevelProgress(
                totalXp);

        var todayCompletions =
            await _dbContext.HabitCompletions
                .AsNoTracking()
                .Where(completion =>
                    completion.UserId == userId
                    && completion.CompletedDate == localDate
                    && completion.UndoneAtUtc == null)
                .Select(completion =>
                    new
                    {
                        completion.Id,
                        completion.HabitId
                    })
                .ToListAsync(cancellationToken);

        var todayCompletionIds =
            todayCompletions
                .Select(completion => completion.Id)
                .ToList();

        var todayCompletedHabitIds =
            todayCompletions
                .Select(completion => completion.HabitId)
                .ToHashSet();

        var xpEarnedToday =
            await _dbContext.XpTransactions
                .Where(transaction =>
                    transaction.UserId == userId
                    && todayCompletionIds.Contains(
                        transaction.HabitCompletionId))
                .SumAsync(
                    transaction => transaction.Amount,
                    cancellationToken);

        var activeHabits =
            await _dbContext.Habits
                .AsNoTracking()
                .Where(habit =>
                    habit.UserId == userId
                    && habit.IsActive)
                .Include(habit =>
                    habit.HabitConfigurationVersions)
                .Include(habit =>
                    habit.HabitCompletions)
                .AsSplitQuery()
                .OrderBy(habit => habit.Name)
                .ThenBy(habit => habit.Id)
                .ToListAsync(cancellationToken);

        var habitStreakCalculations =
            activeHabits
                .Select(habit =>
                    new
                    {
                        Habit = habit,
                        Streak =
                            _streakService.CalculateHabitStreak(
                                localDate,
                                settings.WeekStartsOn,
                                habit.HabitConfigurationVersions
                                    .ToList(),
                                habit.HabitCompletions
                                    .ToList())
                    })
                .ToList();

        var totalDailyHabits =
            habitStreakCalculations.Count(item =>
                item.Streak.FrequencyType
                    == HabitFrequencyType.Daily);

        var completedDailyHabits =
            habitStreakCalculations.Count(item =>
                item.Streak.FrequencyType
                    == HabitFrequencyType.Daily
                && todayCompletedHabitIds.Contains(
                    item.Habit.Id));

        var habitStreaks =
            habitStreakCalculations
                .Select(item =>
                    new HabitStreakResponse
                    {
                        HabitId = item.Habit.Id,
                        HabitName = item.Habit.Name,
                        FrequencyType =
                            item.Streak.FrequencyType,
                        CurrentStreak =
                            item.Streak.CurrentStreak,
                        LongestStreak =
                            item.Streak.LongestStreak
                    })
                .ToList();

        return new DashboardResponse
        {
            OverallProgress =
                new OverallProgressResponse
                {
                    TotalXp = totalXp,
                    Level = overallProgress.Level,
                    XpIntoCurrentLevel =
                        overallProgress.XpIntoCurrentLevel,
                    XpNeededForNextLevel =
                        overallProgress.XpNeededForNextLevel
                },
            TodayActivity =
                new TodayActivityResponse
                {
                    LocalDate = localDate,
                    Completions = todayCompletions.Count,
                    XpEarned = xpEarnedToday
                },
            TodayExecution =
                new TodayExecutionResponse
                {
                    CompletedDailyHabits =
                        completedDailyHabits,
                    TotalDailyHabits =
                        totalDailyHabits
                },
            HabitStreaks = habitStreaks
        };
    }
}
