using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.DTOs;
using HabitTracker.Api.Exceptions;

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