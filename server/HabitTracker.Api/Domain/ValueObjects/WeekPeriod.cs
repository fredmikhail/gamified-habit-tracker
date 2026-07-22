namespace HabitTracker.Api.Domain.ValueObjects;

public readonly record struct WeekPeriod(
    DateOnly StartDateInclusive,
    DateOnly EndDateInclusive);
