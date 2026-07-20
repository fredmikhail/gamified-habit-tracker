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

    public CompletionService(
        AppDbContext dbContext,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<CompleteHabitResponse?> CompleteHabitAsync(
        Guid userId,
        Guid habitId,
        CompleteHabitRequest request,
        CancellationToken cancellationToken = default)
    {
        var habit =
            await _dbContext.Habits.SingleOrDefaultAsync(
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

        var notes =
            string.IsNullOrWhiteSpace(request.Notes)
                ? null
                : request.Notes.Trim();

        var completion = new HabitCompletion
        {
            UserId = userId,
            HabitId = habitId,
            CompletedDate = completedDate,
            CompletedAtUtc =
                completedAtUtc.UtcDateTime,
            Notes = notes
        };

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
                }
        };
    }
}
