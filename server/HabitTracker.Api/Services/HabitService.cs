using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.DTOs;
using HabitTracker.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Services;

public sealed class HabitService
{
    private readonly AppDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public HabitService(
        AppDbContext dbContext,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<HabitResponse> CreateHabitAsync(
        Guid userId,
        CreateHabitRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidHabitNameException();
        }

        var frequencyType =
            request.FrequencyType!.Value;

        var targetCount =
            request.TargetCount!.Value;

        var difficulty =
            request.Difficulty!.Value;

        ValidateTargetCount(
            frequencyType,
            targetCount);

        var createdAtUtc = DateTime.UtcNow;

        var habit = new Habit
        {
            UserId = userId,
            Name = name,
            Description =
                NormalizeOptionalText(request.Description),
            Category =
                NormalizeOptionalText(request.Category),
            FrequencyType = frequencyType,
            TargetCount = targetCount,
            Difficulty = difficulty,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc
        };

        _dbContext.Habits.Add(habit);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return CreateHabitResponse(
            habit,
            isCompletedToday: false);
    }

    public async Task<IReadOnlyList<HabitResponse>> GetUserHabitsAsync(
        Guid userId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query =
            _dbContext.Habits
                .AsNoTracking()
                .Where(habit => habit.UserId == userId);

        if (!includeInactive)
        {
            query =
                query.Where(habit => habit.IsActive);
        }

        var habits =
            await query
                .OrderByDescending(habit => habit.IsActive)
                .ThenByDescending(habit => habit.CreatedAtUtc)
                .ThenBy(habit => habit.Id)
                .ToListAsync(cancellationToken);

        var completedDate =
            await GetUserLocalDateAsync(
                userId,
                cancellationToken);

        var completedHabitIds =
            await _dbContext.HabitCompletions
                .AsNoTracking()
                .Where(completion =>
                    completion.UserId == userId
                    && completion.CompletedDate
                        == completedDate)
                .Select(completion =>
                    completion.HabitId)
                .ToListAsync(cancellationToken);

        var completedHabitIdSet =
            completedHabitIds.ToHashSet();

        return habits
            .Select(habit =>
                CreateHabitResponse(
                    habit,
                    completedHabitIdSet.Contains(
                        habit.Id)))
            .ToList();
    }

    public async Task<HabitResponse?> UpdateHabitAsync(
        Guid userId,
        Guid habitId,
        UpdateHabitRequest request,
        CancellationToken cancellationToken = default)
    {
        var habit =
            await _dbContext.Habits
                .SingleOrDefaultAsync(
                    habit =>
                        habit.Id == habitId
                        && habit.UserId == userId,
                    cancellationToken);

        if (habit is null)
        {
            return null;
        }

        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidHabitNameException();
        }

        var frequencyType =
            request.FrequencyType!.Value;

        var targetCount =
            request.TargetCount!.Value;

        var difficulty =
            request.Difficulty!.Value;

        ValidateTargetCount(
            frequencyType,
            targetCount);

        habit.Name = name;
        habit.Description =
            NormalizeOptionalText(request.Description);
        habit.Category =
            NormalizeOptionalText(request.Category);
        habit.FrequencyType = frequencyType;
        habit.TargetCount = targetCount;
        habit.Difficulty = difficulty;
        habit.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var isCompletedToday =
            await IsCompletedTodayAsync(
                userId,
                habitId,
                cancellationToken);

        return CreateHabitResponse(
            habit,
            isCompletedToday);
    }

    public async Task<HabitResponse?> DeactivateHabitAsync(
        Guid userId,
        Guid habitId,
        CancellationToken cancellationToken = default)
    {
        var habit =
            await _dbContext.Habits
                .SingleOrDefaultAsync(
                    habit =>
                        habit.Id == habitId
                        && habit.UserId == userId,
                    cancellationToken);

        if (habit is null)
        {
            return null;
        }

        if (habit.IsActive)
        {
            habit.IsActive = false;
            habit.UpdatedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        var isCompletedToday =
            await IsCompletedTodayAsync(
                userId,
                habitId,
                cancellationToken);

        return CreateHabitResponse(
            habit,
            isCompletedToday);
    }

    public async Task<HabitResponse?> GetUserHabitAsync(
        Guid userId,
        Guid habitId,
        CancellationToken cancellationToken = default)
    {
        var habit =
            await _dbContext.Habits
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    habit =>
                        habit.Id == habitId
                        && habit.UserId == userId,
                    cancellationToken);

        if (habit is null)
        {
            return null;
        }

        var isCompletedToday =
            await IsCompletedTodayAsync(
                userId,
                habitId,
                cancellationToken);

        return CreateHabitResponse(
            habit,
            isCompletedToday);
    }

    private async Task<DateOnly> GetUserLocalDateAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var timeZoneId =
            await _dbContext.UserSettings
                .Where(settings =>
                    settings.UserId == userId)
                .Select(settings =>
                    settings.TimeZone)
                .SingleAsync(cancellationToken);

        return LocalDateCalculator.GetLocalDate(
            _timeProvider.GetUtcNow(),
            timeZoneId);
    }

    private async Task<bool> IsCompletedTodayAsync(
        Guid userId,
        Guid habitId,
        CancellationToken cancellationToken)
    {
        var completedDate =
            await GetUserLocalDateAsync(
                userId,
                cancellationToken);

        return await _dbContext.HabitCompletions
            .AsNoTracking()
            .AnyAsync(
                completion =>
                    completion.UserId == userId
                    && completion.HabitId == habitId
                    && completion.CompletedDate
                        == completedDate,
                cancellationToken);
    }

    private static void ValidateTargetCount(
        HabitFrequencyType frequencyType,
        int targetCount)
    {
        var isValid =
            frequencyType switch
            {
                HabitFrequencyType.Daily =>
                    targetCount == 1,

                HabitFrequencyType.Weekly =>
                    targetCount is >= 1 and <= 7,

                _ => false
            };

        if (!isValid)
        {
            throw new InvalidHabitTargetCountException();
        }
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static HabitResponse CreateHabitResponse(
        Habit habit,
        bool isCompletedToday)
    {
        return new HabitResponse
        {
            Id = habit.Id,
            Name = habit.Name,
            Description = habit.Description,
            Category = habit.Category,
            FrequencyType = habit.FrequencyType,
            TargetCount = habit.TargetCount,
            Difficulty = habit.Difficulty,
            IsActive = habit.IsActive,
            IsCompletedToday = isCompletedToday,
            CreatedAtUtc = habit.CreatedAtUtc,
            UpdatedAtUtc = habit.UpdatedAtUtc
        };
    }
}
