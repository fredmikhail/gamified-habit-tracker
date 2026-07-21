using HabitTracker.Api.Domain.Enums;

namespace HabitTracker.Api.Domain.Entities;

public sealed class UserSettings
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid UserId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string TimeZone { get; set; } = string.Empty;

    public WeekStartDay WeekStartsOn { get; set; }
        = WeekStartDay.Monday;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public User User { get; set; } = null!;
}
