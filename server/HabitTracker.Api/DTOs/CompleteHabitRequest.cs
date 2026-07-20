using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Api.DTOs;

public sealed class CompleteHabitRequest
{
    [MaxLength(500)]
    public string? Notes { get; set; }
}
