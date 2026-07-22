namespace HabitTracker.Api.DTOs;

public sealed class TodayActivityResponse
{
    public DateOnly LocalDate { get; set; }

    public int Completions { get; set; }

    public int XpEarned { get; set; }
}
