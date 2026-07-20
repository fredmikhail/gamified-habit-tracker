namespace HabitTracker.Api.DTOs;

public sealed class CompleteHabitResponse
{
    public required HabitCompletionResponse Completion { get; set; }
}
