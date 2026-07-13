namespace HabitTracker.Api.DTOs;

public sealed record HealthResponse(
    string Status,
    DateTime CheckedAtUtc);