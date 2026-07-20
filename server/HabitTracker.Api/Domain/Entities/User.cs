namespace HabitTracker.Api.Domain.Entities;

public sealed class User
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public string Email { get; set; } = string.Empty;

    public string NormalizedEmail { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string NormalizedUsername { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? LastLoginAtUtc { get; set; }

    public UserSettings UserSettings { get; set; } = null!;

    public ICollection<Habit> Habits { get; set; }
        = new List<Habit>();

    public ICollection<HabitCompletion> HabitCompletions { get; set; }
        = new List<HabitCompletion>();

    public ICollection<UserAttribute> UserAttributes { get; set; }
        = new List<UserAttribute>();

    public ICollection<XpTransaction> XpTransactions { get; set; }
        = new List<XpTransaction>();
}
