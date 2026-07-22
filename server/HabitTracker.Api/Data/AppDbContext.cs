using HabitTracker.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<UserSettings> UserSettings => Set<UserSettings>();

    public DbSet<Habit> Habits => Set<Habit>();

    public DbSet<HabitConfigurationVersion>
    HabitConfigurationVersions =>
        Set<HabitConfigurationVersion>();

    public DbSet<HabitCompletion> HabitCompletions =>
        Set<HabitCompletion>();

    public DbSet<UserAttribute> UserAttributes =>
        Set<UserAttribute>();

    public DbSet<HabitAttributeReward> HabitAttributeRewards =>
        Set<HabitAttributeReward>();

    public DbSet<XpTransaction> XpTransactions =>
        Set<XpTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}
