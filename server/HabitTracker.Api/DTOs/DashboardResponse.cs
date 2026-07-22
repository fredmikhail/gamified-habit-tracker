namespace HabitTracker.Api.DTOs;

public sealed class DashboardResponse
{
    public OverallProgressResponse OverallProgress { get; set; }
        = new();

    public TodayActivityResponse TodayActivity { get; set; }
        = new();

    public TodayExecutionResponse TodayExecution { get; set; }
        = new();
}
