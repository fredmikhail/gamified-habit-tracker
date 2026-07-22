namespace HabitTracker.Api.Domain.ValueObjects;

public readonly record struct HabitStreakResult(
    int CurrentStreak,
    int LongestStreak);
