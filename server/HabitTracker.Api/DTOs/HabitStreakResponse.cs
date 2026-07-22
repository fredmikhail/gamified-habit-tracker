using HabitTracker.Api.Domain.Enums;

namespace HabitTracker.Api.DTOs;

public sealed class HabitStreakResponse
{
    public Guid HabitId { get; set; }

    public string HabitName { get; set; } = string.Empty;

    public HabitFrequencyType FrequencyType { get; set; }

    public int CurrentStreak { get; set; }

    public int LongestStreak { get; set; }
}
