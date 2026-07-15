namespace HabitTracker.Api.DTOs;

public sealed class AuthResponse
{
    public CurrentUserResponse User { get; set; } = new();
}
