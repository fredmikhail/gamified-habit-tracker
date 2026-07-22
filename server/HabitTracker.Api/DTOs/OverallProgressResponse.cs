namespace HabitTracker.Api.DTOs;

public sealed class OverallProgressResponse
{
    public int TotalXp { get; set; }

    public int Level { get; set; }

    public int XpIntoCurrentLevel { get; set; }

    public int XpNeededForNextLevel { get; set; }
}
