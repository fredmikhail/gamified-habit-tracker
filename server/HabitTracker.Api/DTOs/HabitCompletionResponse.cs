namespace HabitTracker.Api.DTOs;

public sealed class HabitCompletionResponse
{
    public Guid Id { get; set; }

    public Guid HabitId { get; set; }

    public DateOnly CompletedDate { get; set; }

    public DateTime CompletedAtUtc { get; set; }

    public string? Notes { get; set; }
}
