namespace HabitTracker.Api.DTOs;

public sealed class DashboardResponse
{
    public OverallProgressResponse OverallProgress { get; set; }
        = new();
}
