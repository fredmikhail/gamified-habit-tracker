using HabitTracker.Api.Domain.Enums;

namespace HabitTracker.Api.DTOs;

public sealed class DashboardHabitResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public HabitCategory Category { get; set; }

    public HabitFrequencyType FrequencyType { get; set; }

    public int TargetCount { get; set; }

    public HabitDifficulty Difficulty { get; set; }

    public IReadOnlyList<HabitAttributeRewardResponse>
        AttributeRewards
    { get; set; } = [];

    public bool IsCompletedToday { get; set; }

    public int CurrentStreak { get; set; }

    public int LongestStreak { get; set; }
}
