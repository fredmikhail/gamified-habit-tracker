using System.ComponentModel.DataAnnotations;
using HabitTracker.Api.Domain.Enums;

namespace HabitTracker.Api.DTOs;

public sealed class CreateHabitRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [EnumDataType(typeof(HabitCategory))]
    public HabitCategory? Category { get; set; }

    [Required]
    [EnumDataType(typeof(HabitFrequencyType))]
    public HabitFrequencyType? FrequencyType { get; set; }

    [Required]
    [Range(1, 7)]
    public int? TargetCount { get; set; }

    [Required]
    [EnumDataType(typeof(HabitDifficulty))]
    public HabitDifficulty? Difficulty { get; set; }
}
