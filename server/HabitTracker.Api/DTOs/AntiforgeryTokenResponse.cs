namespace HabitTracker.Api.DTOs;

public sealed class AntiforgeryTokenResponse
{
    public string RequestToken { get; set; } = string.Empty;
}
