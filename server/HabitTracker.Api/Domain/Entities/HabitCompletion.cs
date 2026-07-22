namespace HabitTracker.Api.Domain.Entities;

public sealed class HabitCompletion
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid UserId { get; set; }

    public Guid HabitId { get; set; }

    public Guid HabitConfigurationVersionId { get; set; }

    public DateOnly CompletedDate { get; set; }

    public DateTime CompletedAtUtc { get; set; }

    public DateTime? UndoneAtUtc { get; set; }

    public string? Notes { get; set; }

    public User User { get; set; } = null!;

    public Habit Habit { get; set; } = null!;

    public HabitConfigurationVersion HabitConfigurationVersion { get; set; }
        = null!;

    public ICollection<XpTransaction> XpTransactions { get; set; }
        = new List<XpTransaction>();
}
