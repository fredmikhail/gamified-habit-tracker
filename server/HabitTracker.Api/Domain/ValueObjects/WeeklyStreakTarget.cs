namespace HabitTracker.Api.Domain.ValueObjects;

public readonly record struct WeeklyStreakTarget(
    DateOnly EffectiveFromDate,
    DateOnly? EffectiveToDateExclusive,
    int TargetCount);
