namespace HabitTracker.Api.DTOs;

public sealed class CompleteHabitResponse
{
    public required HabitCompletionResponse Completion { get; set; }

    public IReadOnlyList<HabitAttributeRewardResponse>
        Rewards
    { get; set; }
            = Array.Empty<HabitAttributeRewardResponse>();
}
