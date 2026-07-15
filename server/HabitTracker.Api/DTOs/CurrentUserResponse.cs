namespace HabitTracker.Api.DTOs;

public sealed class CurrentUserResponse
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string TimeZone { get; set; } = string.Empty;
}
