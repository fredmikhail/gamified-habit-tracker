using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using HabitTracker.Api.DTOs;
using HabitTracker.Api.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HabitTracker.Api.Services;

public sealed class CompletionService
{
    private readonly AppDbContext _dbContext;
    private readonly TimeProvider _timeProvider;
    private readonly AttributeService _attributeService;
    private readonly XpService _xpService;

    public CompletionService(
        AppDbContext dbContext,
        TimeProvider timeProvider,
        AttributeService attributeService,
        XpService xpService)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
        _attributeService = attributeService;
        _xpService = xpService;
    }

    public async Task<CompleteHabitResponse?> CompleteHabitAsync(
        Guid userId,
        Guid habitId,
        CompleteHabitRequest request,
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

        if (!habit.IsActive)
        {
            throw new InactiveHabitException();
        }

        var timeZoneId =
            await _dbContext.UserSettings
                .Where(settings =>
                    settings.UserId == userId)
                .Select(settings =>
                    settings.TimeZone)
                .SingleAsync(cancellationToken);

        var completedAtUtc =
            _timeProvider.GetUtcNow();

        var completedDate =
            LocalDateCalculator.GetLocalDate(
                completedAtUtc,
                timeZoneId);

        var alreadyCompleted =
            await _dbContext.HabitCompletions.AnyAsync(
                completion =>
                    completion.HabitId == habitId
                    && completion.CompletedDate
                        == completedDate,
                cancellationToken);

        if (alreadyCompleted)
        {
            throw new HabitAlreadyCompletedException();
        }

        var configuration =
            await _dbContext.HabitConfigurationVersions
                .SingleOrDefaultAsync(
                    configuration =>
                        configuration.HabitId == habitId
                        && configuration.EffectiveFromDate
                            <= completedDate
                        && (
                            configuration
                                .EffectiveToDateExclusive == null
                            || completedDate
                                < configuration
                                    .EffectiveToDateExclusive),
                    cancellationToken)
            ?? throw new InvalidOperationException(
                "The habit does not have a configuration effective on the completion date.");

        var notes =
            string.IsNullOrWhiteSpace(request.Notes)
                ? null
                : request.Notes.Trim();

        var completion = new HabitCompletion
        {
            UserId = userId,
            HabitId = habitId,
            HabitConfigurationVersionId =
                configuration.Id,
            HabitConfigurationVersion =
                configuration,
            CompletedDate = completedDate,
            CompletedAtUtc =
                completedAtUtc.UtcDateTime,
            Notes = notes
        };

        var rewards =
            CreateRewards(configuration);

        await _attributeService
            .ApplyCompletionRewardsAsync(
                userId,
                completion,
                rewards,
                completion.CompletedAtUtc,
                cancellationToken);

        _dbContext.HabitCompletions.Add(completion);

        try
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateException exception)
            when (
                exception.InnerException
                    is PostgresException postgresException
                && postgresException.SqlState
                    == PostgresErrorCodes.UniqueViolation
                && postgresException.ConstraintName
                    == "ix_habit_completions_habit_id_completed_date")
        {
            throw new HabitAlreadyCompletedException();
        }

        return new CompleteHabitResponse
        {
            Completion =
                new HabitCompletionResponse
                {
                    Id = completion.Id,
                    HabitId = completion.HabitId,
                    CompletedDate =
                        completion.CompletedDate,
                    CompletedAtUtc =
                        completion.CompletedAtUtc,
                    Notes = completion.Notes
                },
            Rewards =
                CreateRewardResponses(rewards)
        };
    }

    public async Task<bool> UndoTodayAsync(
        Guid userId,
        Guid habitId,
        CancellationToken cancellationToken = default)
    {
        var habitExists =
            await _dbContext.Habits.AnyAsync(
                habit =>
                    habit.Id == habitId
                    && habit.UserId == userId,
                cancellationToken);

        if (!habitExists)
        {
            return false;
        }

        var timeZoneId =
            await _dbContext.UserSettings
                .Where(settings =>
                    settings.UserId == userId)
                .Select(settings =>
                    settings.TimeZone)
                .SingleAsync(cancellationToken);

        var currentUtc =
            _timeProvider.GetUtcNow();

        var completedDate =
            LocalDateCalculator.GetLocalDate(
                currentUtc,
                timeZoneId);

        var completion =
            await _dbContext.HabitCompletions
                .SingleOrDefaultAsync(
                    completion =>
                        completion.HabitId == habitId
                        && completion.CompletedDate
                            == completedDate,
                    cancellationToken);

        if (completion is null)
        {
            return false;
        }

        await _attributeService
            .ReverseCompletionRewardsAsync(
                userId,
                completion,
                currentUtc.UtcDateTime,
                cancellationToken);

        _dbContext.HabitCompletions.Remove(
            completion);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    private List<HabitAttributeReward> CreateRewards(
        HabitConfigurationVersion configuration)
    {
        return _xpService
            .CalculateRewards(
                configuration.Category,
                configuration.Difficulty)
            .Select(reward =>
                new HabitAttributeReward
                {
                    HabitId = configuration.HabitId,
                    AttributeType = reward.Key,
                    XpAmount = reward.Value
                })
            .ToList();
    }

    private static IReadOnlyList<
        HabitAttributeRewardResponse>
        CreateRewardResponses(
            IEnumerable<HabitAttributeReward> rewards)
    {
        return rewards
            .OrderByDescending(reward =>
                reward.XpAmount)
            .ThenBy(reward =>
                reward.AttributeType)
            .Select(reward =>
                new HabitAttributeRewardResponse
                {
                    AttributeType =
                        reward.AttributeType,
                    XpAmount = reward.XpAmount
                })
            .ToList();
    }
}
