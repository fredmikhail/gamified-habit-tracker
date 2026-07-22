using HabitTracker.Api.Domain.Enums;

namespace HabitTracker.Api.Domain.ValueObjects;

public readonly record struct HabitStreakSummary(
    HabitFrequencyType FrequencyType,
    int CurrentStreak,
    int LongestStreak);
