namespace HabitTracker.Api.Domain.ValueObjects;

public sealed record LevelProgress(
    int Level,
    int XpIntoCurrentLevel,
    int XpNeededForNextLevel);
