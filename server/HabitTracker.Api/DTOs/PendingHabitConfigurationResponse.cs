using HabitTracker.Api.Domain.Enums;

namespace HabitTracker.Api.DTOs;

public sealed class PendingHabitConfigurationResponse
{
    public DateOnly EffectiveFromDate { get; set; }

    public HabitCategory Category { get; set; }

    public HabitFrequencyType FrequencyType { get; set; }

    public int TargetCount { get; set; }

    public HabitDifficulty Difficulty { get; set; }
}
