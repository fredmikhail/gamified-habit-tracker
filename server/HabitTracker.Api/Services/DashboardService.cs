using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Services;

public sealed class DashboardService
{
    private readonly AppDbContext _dbContext;
    private readonly XpService _xpService;
    private readonly TimeProvider _timeProvider;

    public DashboardService(
        AppDbContext dbContext,
        XpService xpService,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _xpService = xpService;
        _timeProvider = timeProvider;
    }

    public async Task<DashboardResponse> GetDashboardAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var timeZoneId =
            await _dbContext.UserSettings
                .Where(settings =>
                    settings.UserId == userId)
                .Select(settings =>
                    settings.TimeZone)
                .SingleAsync(cancellationToken);

        var localDate =
            LocalDateCalculator.GetLocalDate(
                _timeProvider.GetUtcNow(),
                timeZoneId);

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
                .Distinct()
                .ToList();

        var xpEarnedToday =
            await _dbContext.XpTransactions
                .Where(transaction =>
                    transaction.UserId == userId
                    && todayCompletionIds.Contains(
                        transaction.HabitCompletionId))
                .SumAsync(
                    transaction => transaction.Amount,
                    cancellationToken);

        var totalDailyHabits =
            await _dbContext.Habits
                .CountAsync(
                    habit =>
                        habit.UserId == userId
                        && habit.IsActive
                        && habit.FrequencyType
                            == HabitFrequencyType.Daily,
                    cancellationToken);

        var completedDailyHabits =
            await _dbContext.Habits
                .CountAsync(
                    habit =>
                        habit.UserId == userId
                        && habit.IsActive
                        && habit.FrequencyType
                            == HabitFrequencyType.Daily
                        && todayCompletedHabitIds.Contains(
                            habit.Id),
                    cancellationToken);

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
                }
        };
    }
}
