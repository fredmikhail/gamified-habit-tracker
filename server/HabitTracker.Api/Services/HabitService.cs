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

    public HabitService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
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

        return CreateHabitResponse(habit);
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

        return habits
            .Select(CreateHabitResponse)
            .ToList();
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

        return CreateHabitResponse(habit);
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
        Habit habit)
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
            CreatedAtUtc = habit.CreatedAtUtc,
            UpdatedAtUtc = habit.UpdatedAtUtc
        };
    }
}