using HabitTracker.Api.Domain.Enums;

namespace HabitTracker.Api.Domain.Entities;

public sealed class Habit
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Category { get; set; }

    public HabitFrequencyType FrequencyType { get; set; }

    public int TargetCount { get; set; }

    public HabitDifficulty Difficulty { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public User User { get; set; } = null!;
}