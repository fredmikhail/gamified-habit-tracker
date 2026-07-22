using System.Text.Json.Serialization;
using HabitTracker.Api.Domain.Enums;

namespace HabitTracker.Api.DTOs;

public sealed class HabitResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public HabitCategory Category { get; set; }

    public HabitFrequencyType FrequencyType { get; set; }

    public int TargetCount { get; set; }

    public HabitDifficulty Difficulty { get; set; }

    [JsonIgnore(
        Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PendingHabitConfigurationResponse?
        PendingConfiguration
    { get; set; }

    public IReadOnlyList<HabitAttributeRewardResponse>
        AttributeRewards
    { get; set; }
        = Array.Empty<HabitAttributeRewardResponse>();

    public bool IsActive { get; set; }

    public bool IsCompletedToday { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
