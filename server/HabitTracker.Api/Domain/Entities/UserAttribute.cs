using HabitTracker.Api.Domain.Enums;

namespace HabitTracker.Api.Domain.Entities;

public sealed class UserAttribute
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid UserId { get; set; }

    public AttributeType AttributeType { get; set; }

    public int CurrentXp { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public User User { get; set; } = null!;
}