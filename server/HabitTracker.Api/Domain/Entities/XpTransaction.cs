using HabitTracker.Api.Domain.Enums;

namespace HabitTracker.Api.Domain.Entities;

public sealed class XpTransaction
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid UserId { get; set; }

    public Guid HabitCompletionId { get; set; }

    public AttributeType AttributeType { get; set; }

    public int Amount { get; set; }

    public string Reason { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public User User { get; set; } = null!;

    public HabitCompletion HabitCompletion { get; set; } = null!;
}
