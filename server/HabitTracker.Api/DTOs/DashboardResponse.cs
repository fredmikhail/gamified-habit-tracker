namespace HabitTracker.Api.DTOs;

public sealed class DashboardResponse
{
    public OverallProgressResponse OverallProgress { get; set; }
        = new();

    public TodayActivityResponse TodayActivity { get; set; }
        = new();

    public TodayExecutionResponse TodayExecution { get; set; }
        = new();

    public IReadOnlyList<DashboardHabitResponse> TodayHabits { get; set; }
        = [];

    public IReadOnlyList<UserAttributeResponse> Attributes { get; set; }
        = [];

    public IReadOnlyList<HabitStreakResponse> HabitStreaks { get; set; }
        = [];
}
