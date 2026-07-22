using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.Services;
using Microsoft.EntityFrameworkCore;


namespace HabitTracker.Tests.Services;

public sealed class DashboardServiceTests
{
    private static readonly DateTimeOffset FixedUtcNow =
        new(
            2026,
            7,
            22,
            14,
            0,
            0,
            TimeSpan.Zero);

    [Fact]
    public async Task GetDashboardAsync_WhenUserHasNoProgress_ReturnsInitialDashboard()
    {
        await using var dbContext =
            CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        await dbContext.SaveChangesAsync();

        var dashboardService =
            CreateDashboardService(dbContext);

        var response =
            await dashboardService.GetDashboardAsync(
                userId);

        Assert.Equal(
            0,
            response.OverallProgress.TotalXp);

        Assert.Equal(
            1,
            response.OverallProgress.Level);

        Assert.Equal(
            0,
            response.OverallProgress
                .XpIntoCurrentLevel);

        Assert.Equal(
            200,
            response.OverallProgress
                .XpNeededForNextLevel);

        Assert.Equal(
            new DateOnly(2026, 7, 22),
            response.TodayActivity.LocalDate);

        Assert.Equal(
            0,
            response.TodayActivity.Completions);

        Assert.Equal(
            0,
            response.TodayActivity.XpEarned);

        Assert.Equal(
            0,
            response.TodayExecution
                .CompletedDailyHabits);

        Assert.Equal(
            0,
            response.TodayExecution
                .TotalDailyHabits);
    }

    [Fact]
    public async Task GetDashboardAsync_WhenUserHasXp_SumsOnlyOwnedTransactions()
    {
        await using var dbContext =
            CreateDbContext();

        var userId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        var createdAtUtc = FixedUtcNow.UtcDateTime;

        dbContext.XpTransactions.AddRange(
            CreateTransaction(
                userId,
                Guid.CreateVersion7(),
                AttributeType.Fitness,
                140,
                createdAtUtc),
            CreateTransaction(
                userId,
                Guid.CreateVersion7(),
                AttributeType.Discipline,
                100,
                createdAtUtc.AddMinutes(1)),
            CreateTransaction(
                userId,
                Guid.CreateVersion7(),
                AttributeType.Focus,
                60,
                createdAtUtc.AddMinutes(2)),
            CreateTransaction(
                otherUserId,
                Guid.CreateVersion7(),
                AttributeType.Mind,
                999,
                createdAtUtc.AddMinutes(3)));

        await dbContext.SaveChangesAsync();

        var dashboardService =
            CreateDashboardService(dbContext);

        var response =
            await dashboardService.GetDashboardAsync(
                userId);

        Assert.Equal(
            300,
            response.OverallProgress.TotalXp);

        Assert.Equal(
            2,
            response.OverallProgress.Level);

        Assert.Equal(
            100,
            response.OverallProgress
                .XpIntoCurrentLevel);

        Assert.Equal(
            250,
            response.OverallProgress
                .XpNeededForNextLevel);
    }

    [Fact]
    public async Task GetDashboardAsync_WhenUserHasTodayActivity_ReturnsOwnedActivityAndDailyExecution()
    {
        await using var dbContext =
            CreateDbContext();

        var userId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        AddUserWithSettings(
            dbContext,
            otherUserId);

        var completedDailyHabit =
            CreateHabit(
                userId,
                HabitFrequencyType.Daily,
                isActive: true);

        var incompleteDailyHabit =
            CreateHabit(
                userId,
                HabitFrequencyType.Daily,
                isActive: true);

        var completedWeeklyHabit =
            CreateHabit(
                userId,
                HabitFrequencyType.Weekly,
                isActive: true);

        var otherUserDailyHabit =
            CreateHabit(
                otherUserId,
                HabitFrequencyType.Daily,
                isActive: true);

        dbContext.Habits.AddRange(
            completedDailyHabit,
            incompleteDailyHabit,
            completedWeeklyHabit,
            otherUserDailyHabit);

        var dailyCompletion =
            CreateCompletion(
                userId,
                completedDailyHabit.Id,
                new DateOnly(2026, 7, 22));

        var weeklyCompletion =
            CreateCompletion(
                userId,
                completedWeeklyHabit.Id,
                new DateOnly(2026, 7, 22));

        var yesterdayCompletion =
            CreateCompletion(
                userId,
                completedDailyHabit.Id,
                new DateOnly(2026, 7, 21));

        var otherUserCompletion =
            CreateCompletion(
                otherUserId,
                otherUserDailyHabit.Id,
                new DateOnly(2026, 7, 22));

        dbContext.HabitCompletions.AddRange(
            dailyCompletion,
            weeklyCompletion,
            yesterdayCompletion,
            otherUserCompletion);

        var createdAtUtc = FixedUtcNow.UtcDateTime;

        dbContext.XpTransactions.AddRange(
            CreateTransaction(
                userId,
                dailyCompletion.Id,
                AttributeType.Discipline,
                10,
                createdAtUtc),
            CreateTransaction(
                userId,
                weeklyCompletion.Id,
                AttributeType.Focus,
                20,
                createdAtUtc.AddMinutes(1)),
            CreateTransaction(
                userId,
                yesterdayCompletion.Id,
                AttributeType.Mind,
                99,
                createdAtUtc.AddDays(-1)),
            CreateTransaction(
                otherUserId,
                otherUserCompletion.Id,
                AttributeType.Fitness,
                999,
                createdAtUtc.AddMinutes(2)));

        await dbContext.SaveChangesAsync();

        var dashboardService =
            CreateDashboardService(dbContext);

        var response =
            await dashboardService.GetDashboardAsync(
                userId);

        Assert.Equal(
            2,
            response.TodayActivity.Completions);

        Assert.Equal(
            30,
            response.TodayActivity.XpEarned);

        Assert.Equal(
            1,
            response.TodayExecution
                .CompletedDailyHabits);

        Assert.Equal(
            2,
            response.TodayExecution
                .TotalDailyHabits);
    }

    [Fact]
    public async Task GetDashboardAsync_WhenHabitHasCompletions_ReturnsHabitStreak()
    {
        await using var dbContext =
            CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        var habit =
            CreateHabit(
                userId,
                HabitFrequencyType.Daily,
                isActive: true);

        dbContext.Habits.Add(habit);

        dbContext.HabitCompletions.AddRange(
            CreateCompletion(
                userId,
                habit.Id,
                new DateOnly(2026, 7, 20)),
            CreateCompletion(
                userId,
                habit.Id,
                new DateOnly(2026, 7, 21)));

        await dbContext.SaveChangesAsync();

        var dashboardService =
            CreateDashboardService(dbContext);

        var response =
            await dashboardService.GetDashboardAsync(
                userId);

        var habitStreak =
            Assert.Single(response.HabitStreaks);

        Assert.Equal(
            habit.Id,
            habitStreak.HabitId);

        Assert.Equal(
            habit.Name,
            habitStreak.HabitName);

        Assert.Equal(
            HabitFrequencyType.Daily,
            habitStreak.FrequencyType);

        Assert.Equal(
            2,
            habitStreak.CurrentStreak);

        Assert.Equal(
            2,
            habitStreak.LongestStreak);
    }

    private static DashboardService CreateDashboardService(
    AppDbContext dbContext)
    {
        return new DashboardService(
            dbContext,
            new XpService(),
            new StreakService(),
            new FixedTimeProvider(FixedUtcNow));
    }

    private static void AddUserWithSettings(
        AppDbContext dbContext,
        Guid userId)
    {
        var createdAtUtc =
            new DateTime(
                2026,
                7,
                1,
                12,
                0,
                0,
                DateTimeKind.Utc);

        var uniqueSuffix =
            userId.ToString("N");

        var user = new User
        {
            Id = userId,
            Email =
                $"user_{uniqueSuffix}@example.com",
            NormalizedEmail =
                $"USER_{uniqueSuffix}@EXAMPLE.COM",
            Username =
                $"user_{uniqueSuffix[..8]}",
            NormalizedUsername =
                $"USER_{uniqueSuffix[..8]}",
            PasswordHash =
                "test-password-hash",
            CreatedAtUtc = createdAtUtc
        };

        var settings = new UserSettings
        {
            UserId = userId,
            DisplayName =
                $"User {uniqueSuffix[..8]}",
            TimeZone = "America/Toronto",
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc,
            User = user
        };

        user.UserSettings = settings;

        dbContext.Users.Add(user);
    }

    private static Habit CreateHabit(
    Guid userId,
    HabitFrequencyType frequencyType,
    bool isActive)
    {
        var createdAtUtc =
            FixedUtcNow.UtcDateTime.AddDays(-7);

        var habit =
            new Habit
            {
                UserId = userId,
                Name = "Dashboard test habit",
                Category =
                    HabitCategory.GeneralGrowth,
                FrequencyType = frequencyType,
                TargetCount =
                    frequencyType == HabitFrequencyType.Daily
                        ? 1
                        : 3,
                Difficulty =
                    HabitDifficulty.Medium,
                IsActive = isActive,
                CreatedAtUtc = createdAtUtc,
                UpdatedAtUtc = createdAtUtc
            };

        habit.HabitConfigurationVersions.Add(
            new HabitConfigurationVersion
            {
                HabitId = habit.Id,
                VersionNumber = 1,
                Category = habit.Category,
                FrequencyType = habit.FrequencyType,
                TargetCount = habit.TargetCount,
                Difficulty = habit.Difficulty,
                EffectiveFromDate =
                    new DateOnly(2026, 7, 15),
                CreatedAtUtc = createdAtUtc
            });

        return habit;
    }

    private static HabitCompletion CreateCompletion(
        Guid userId,
        Guid habitId,
        DateOnly completedDate)
    {
        return new HabitCompletion
        {
            UserId = userId,
            HabitId = habitId,
            CompletedDate = completedDate,
            CompletedAtUtc =
                FixedUtcNow.UtcDateTime
        };
    }

    private static XpTransaction CreateTransaction(
        Guid userId,
        Guid habitCompletionId,
        AttributeType attributeType,
        int amount,
        DateTime createdAtUtc)
    {
        return new XpTransaction
        {
            UserId = userId,
            HabitCompletionId =
                habitCompletionId,
            AttributeType = attributeType,
            Amount = amount,
            Reason = "Dashboard test",
            CreatedAtUtc = createdAtUtc
        };
    }

    private static AppDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(
                    Guid.NewGuid().ToString())
                .Options;

        return new AppDbContext(options);
    }

    private sealed class FixedTimeProvider
        : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(
            DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }
}
