using HabitTracker.Api.Domain.Enums;

namespace HabitTracker.Api.Domain.Entities;

public sealed class HabitConfigurationVersion
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid HabitId { get; set; }

    public int VersionNumber { get; set; }

    public HabitCategory Category { get; set; }

    public HabitFrequencyType FrequencyType { get; set; }

    public int TargetCount { get; set; }

    public HabitDifficulty Difficulty { get; set; }

    public DateOnly EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDateExclusive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public Habit Habit { get; set; } = null!;

    public ICollection<HabitCompletion> HabitCompletions { get; set; }
        = new List<HabitCompletion>();
}
