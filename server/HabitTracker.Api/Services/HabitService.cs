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
    private readonly XpService _xpService;

    public HabitService(
        AppDbContext dbContext,
        TimeProvider timeProvider,
        XpService xpService)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
        _xpService = xpService;
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

        var category =
            request.Category!.Value;

        var frequencyType =
            request.FrequencyType!.Value;

        var targetCount =
            request.TargetCount!.Value;

        var difficulty =
            request.Difficulty!.Value;

        ValidateTargetCount(
            frequencyType,
            targetCount);

        var createdAtUtc =
            _timeProvider.GetUtcNow();

        var timeZoneId =
            await GetUserTimeZoneIdAsync(
                userId,
                cancellationToken);

        var effectiveFromDate =
            LocalDateCalculator.GetLocalDate(
                createdAtUtc,
                timeZoneId);

        var habit = new Habit
        {
            UserId = userId,
            Name = name,
            Description =
                NormalizeOptionalText(request.Description),
            Category = category,
            FrequencyType = frequencyType,
            TargetCount = targetCount,
            Difficulty = difficulty,
            CreatedAtUtc = createdAtUtc.UtcDateTime,
            UpdatedAtUtc = createdAtUtc.UtcDateTime
        };

        habit.HabitConfigurationVersions.Add(
            new HabitConfigurationVersion
            {
                HabitId = habit.Id,
                VersionNumber = 1,
                Category = category,
                FrequencyType = frequencyType,
                TargetCount = targetCount,
                Difficulty = difficulty,
                EffectiveFromDate =
                    effectiveFromDate,
                CreatedAtUtc =
                    createdAtUtc.UtcDateTime
            });

        SynchronizeAttributeRewards(habit);

        _dbContext.Habits.Add(habit);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return CreateHabitResponse(
    habit,
    isCompletedToday: false,
    effectiveFromDate);
    }

    public async Task<IReadOnlyList<HabitResponse>> GetUserHabitsAsync(
        Guid userId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query =
    _dbContext.Habits
        .AsNoTracking()
        .Include(habit =>
            habit.HabitAttributeRewards)
        .Include(habit =>
            habit.HabitConfigurationVersions)
        .Where(habit =>
            habit.UserId == userId);

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
                habit.Id),
            completedDate))
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
                .Include(habit =>
                    habit.HabitAttributeRewards)
                .Include(habit =>
                    habit.HabitConfigurationVersions)
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

        var category =
            request.Category!.Value;

        var frequencyType =
            request.FrequencyType!.Value;

        var targetCount =
            request.TargetCount!.Value;

        var difficulty =
            request.Difficulty!.Value;

        ValidateTargetCount(
            frequencyType,
            targetCount);

        var settings =
            await GetUserCalendarSettingsAsync(
                userId,
                cancellationToken);

        var updatedAtUtc =
            _timeProvider.GetUtcNow();

        var localDate =
            LocalDateCalculator.GetLocalDate(
                updatedAtUtc,
                settings.TimeZoneId);

        var currentWeek =
            CalendarPeriodCalculator.GetWeekPeriod(
                localDate,
                settings.WeekStartsOn);

        var nextWeekStart =
            currentWeek.EndDateInclusive.AddDays(1);

        var currentConfiguration =
            GetEffectiveConfiguration(
                habit,
                localDate);

        var pendingConfiguration =
            habit.HabitConfigurationVersions
                .SingleOrDefault(configuration =>
                    configuration.EffectiveFromDate
                        > localDate);

        if (pendingConfiguration is not null
            && pendingConfiguration.EffectiveFromDate
                != nextWeekStart)
        {
            throw new InvalidOperationException(
                "The pending habit configuration does not begin at the next week boundary.");
        }

        habit.Name = name;
        habit.Description =
            NormalizeOptionalText(request.Description);
        habit.UpdatedAtUtc =
            updatedAtUtc.UtcDateTime;

        var requestedRulesMatchCurrent =
            HasSameRules(
                currentConfiguration,
                category,
                frequencyType,
                targetCount,
                difficulty);

        if (requestedRulesMatchCurrent)
        {
            if (pendingConfiguration is not null)
            {
                _dbContext.HabitConfigurationVersions.Remove(
                    pendingConfiguration);

                currentConfiguration
                    .EffectiveToDateExclusive = null;
            }
        }
        else if (pendingConfiguration is null)
        {
            currentConfiguration.EffectiveToDateExclusive =
                nextWeekStart;

            var nextVersionNumber =
                habit.HabitConfigurationVersions
                    .Max(configuration =>
                        configuration.VersionNumber)
                + 1;

            habit.HabitConfigurationVersions.Add(
                new HabitConfigurationVersion
                {
                    HabitId = habit.Id,
                    VersionNumber =
                        nextVersionNumber,
                    Category = category,
                    FrequencyType =
                        frequencyType,
                    TargetCount = targetCount,
                    Difficulty = difficulty,
                    EffectiveFromDate =
                        nextWeekStart,
                    CreatedAtUtc =
                        updatedAtUtc.UtcDateTime
                });
        }
        else
        {
            pendingConfiguration.Category =
                category;
            pendingConfiguration.FrequencyType =
                frequencyType;
            pendingConfiguration.TargetCount =
                targetCount;
            pendingConfiguration.Difficulty =
                difficulty;
            pendingConfiguration.CreatedAtUtc =
                updatedAtUtc.UtcDateTime;
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var isCompletedToday =
            await IsCompletedTodayAsync(
                userId,
                habitId,
                cancellationToken);

        return CreateHabitResponse(
            habit,
            isCompletedToday,
            localDate);
    }
    public async Task<HabitResponse?> DeactivateHabitAsync(
        Guid userId,
        Guid habitId,
        CancellationToken cancellationToken = default)
    {
        var habit =
    await _dbContext.Habits
        .Include(habit =>
            habit.HabitAttributeRewards)
        .Include(habit =>
            habit.HabitConfigurationVersions)
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

        var localDate =
    await GetUserLocalDateAsync(
        userId,
        cancellationToken);

        return CreateHabitResponse(
            habit,
            isCompletedToday,
            localDate);
    }

    public async Task<HabitResponse?> GetUserHabitAsync(
        Guid userId,
        Guid habitId,
        CancellationToken cancellationToken = default)
    {
        var habit =
    await _dbContext.Habits
        .AsNoTracking()
        .Include(habit =>
            habit.HabitAttributeRewards)
        .Include(habit =>
            habit.HabitConfigurationVersions)
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

        var localDate =
    await GetUserLocalDateAsync(
        userId,
        cancellationToken);

        return CreateHabitResponse(
            habit,
            isCompletedToday,
            localDate);
    }

    private async Task<UserCalendarSettings>
    GetUserCalendarSettingsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.UserSettings
            .Where(settings =>
                settings.UserId == userId)
            .Select(settings =>
                new UserCalendarSettings(
                    settings.TimeZone,
                    settings.WeekStartsOn))
            .SingleAsync(cancellationToken);
    }

    private static HabitConfigurationVersion
    GetEffectiveConfiguration(
        Habit habit,
        DateOnly date)
    {
        return habit.HabitConfigurationVersions
            .SingleOrDefault(configuration =>
                configuration.EffectiveFromDate
                    <= date
                && (
                    configuration
                        .EffectiveToDateExclusive is null
                    || date
                        < configuration
                            .EffectiveToDateExclusive))
            ?? throw new InvalidOperationException(
                "The habit does not have a configuration effective on the requested date.");
    }

    private static bool HasSameRules(
        HabitConfigurationVersion configuration,
        HabitCategory category,
        HabitFrequencyType frequencyType,
        int targetCount,
        HabitDifficulty difficulty)
    {
        return configuration.Category == category
            && configuration.FrequencyType
                == frequencyType
            && configuration.TargetCount
                == targetCount
            && configuration.Difficulty
                == difficulty;
    }

    private async Task<string> GetUserTimeZoneIdAsync(
    Guid userId,
    CancellationToken cancellationToken)
    {
        return await _dbContext.UserSettings
            .Where(settings =>
                settings.UserId == userId)
            .Select(settings =>
                settings.TimeZone)
            .SingleAsync(cancellationToken);
    }

    private async Task<DateOnly> GetUserLocalDateAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var timeZoneId =
            await GetUserTimeZoneIdAsync(
                userId,
                cancellationToken);

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

    private void SynchronizeAttributeRewards(
    Habit habit)
    {
        var calculatedRewards =
            _xpService.CalculateRewards(
                habit.Category,
                habit.Difficulty);

        var obsoleteRewards =
            habit.HabitAttributeRewards
                .Where(reward =>
                    !calculatedRewards.ContainsKey(
                        reward.AttributeType))
                .ToList();

        _dbContext.HabitAttributeRewards.RemoveRange(
            obsoleteRewards);

        foreach (var calculatedReward in calculatedRewards)
        {
            var existingReward =
                habit.HabitAttributeRewards
                    .SingleOrDefault(reward =>
                        reward.AttributeType
                            == calculatedReward.Key);

            if (existingReward is null)
            {
                habit.HabitAttributeRewards.Add(
                    new HabitAttributeReward
                    {
                        HabitId = habit.Id,
                        AttributeType =
                            calculatedReward.Key,
                        XpAmount =
                            calculatedReward.Value
                    });

                continue;
            }

            existingReward.XpAmount =
                calculatedReward.Value;
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

    private IReadOnlyList<HabitAttributeRewardResponse>
    CreateAttributeRewardResponses(
        HabitConfigurationVersion configuration)
    {
        return _xpService
            .CalculateRewards(
                configuration.Category,
                configuration.Difficulty)
            .OrderByDescending(reward =>
                reward.Value)
            .ThenBy(reward =>
                reward.Key)
            .Select(reward =>
                new HabitAttributeRewardResponse
                {
                    AttributeType = reward.Key,
                    XpAmount = reward.Value
                })
            .ToList();
    }

    private HabitResponse CreateHabitResponse(
    Habit habit,
    bool isCompletedToday,
    DateOnly localDate)
    {
        var effectiveConfiguration =
            GetEffectiveConfiguration(
                habit,
                localDate);

        var pendingConfiguration =
            habit.HabitConfigurationVersions
                .SingleOrDefault(configuration =>
                    configuration.EffectiveFromDate
                        > localDate);

        return new HabitResponse
        {
            Id = habit.Id,
            Name = habit.Name,
            Description = habit.Description,
            Category =
                effectiveConfiguration.Category,
            FrequencyType =
                effectiveConfiguration.FrequencyType,
            TargetCount =
                effectiveConfiguration.TargetCount,
            Difficulty =
                effectiveConfiguration.Difficulty,
            PendingConfiguration =
                pendingConfiguration is null
                    ? null
                    : new PendingHabitConfigurationResponse
                    {
                        EffectiveFromDate =
                            pendingConfiguration
                                .EffectiveFromDate,
                        Category =
                            pendingConfiguration.Category,
                        FrequencyType =
                            pendingConfiguration
                                .FrequencyType,
                        TargetCount =
                            pendingConfiguration.TargetCount,
                        Difficulty =
                            pendingConfiguration.Difficulty
                    },
            AttributeRewards =
                CreateAttributeRewardResponses(
                    effectiveConfiguration),
            IsActive = habit.IsActive,
            IsCompletedToday = isCompletedToday,
            CreatedAtUtc = habit.CreatedAtUtc,
            UpdatedAtUtc = habit.UpdatedAtUtc
        };
    }

    private sealed record UserCalendarSettings(
    string TimeZoneId,
    WeekStartDay WeekStartsOn);
}
