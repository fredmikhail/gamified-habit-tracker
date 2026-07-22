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

        Assert.Empty(response.TodayHabits);

        Assert.Equal(
            Enum.GetValues<AttributeType>(),
            response.Attributes
                .Select(attribute =>
                    attribute.AttributeType)
                .ToArray());

        Assert.All(
            response.Attributes,
            attribute =>
                Assert.Equal(
                    0,
                    attribute.CurrentXp));
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
    public async Task GetDashboardAsync_WhenUserHasTodayActivity_ReturnsOwnedActivityExecutionAndHabits()
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
                isActive: true,
                name: "Completed daily habit",
                category:
                    HabitCategory.LearningAndSkills,
                difficulty:
                    HabitDifficulty.Hard);

        var incompleteDailyHabit =
            CreateHabit(
                userId,
                HabitFrequencyType.Daily,
                isActive: true,
                name: "Incomplete daily habit");

        var completedWeeklyHabit =
            CreateHabit(
                userId,
                HabitFrequencyType.Weekly,
                isActive: true,
                name: "Completed weekly habit");

        var inactiveHabit =
            CreateHabit(
                userId,
                HabitFrequencyType.Daily,
                isActive: false,
                name: "Inactive habit");

        var otherUserDailyHabit =
            CreateHabit(
                otherUserId,
                HabitFrequencyType.Daily,
                isActive: true,
                name: "Other user habit");

        dbContext.Habits.AddRange(
            completedDailyHabit,
            incompleteDailyHabit,
            completedWeeklyHabit,
            inactiveHabit,
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

        Assert.Equal(
            3,
            response.TodayHabits.Count);

        Assert.DoesNotContain(
            response.TodayHabits,
            habit =>
                habit.Id == inactiveHabit.Id);

        Assert.DoesNotContain(
            response.TodayHabits,
            habit =>
                habit.Id == otherUserDailyHabit.Id);

        var completedDailyResponse =
            Assert.Single(
                response.TodayHabits,
                habit =>
                    habit.Id
                    == completedDailyHabit.Id);

        Assert.True(
            completedDailyResponse
                .IsCompletedToday);

        Assert.Equal(
            HabitCategory.LearningAndSkills,
            completedDailyResponse.Category);

        Assert.Equal(
            HabitFrequencyType.Daily,
            completedDailyResponse
                .FrequencyType);

        Assert.Equal(
            1,
            completedDailyResponse.TargetCount);

        Assert.Equal(
            HabitDifficulty.Hard,
            completedDailyResponse.Difficulty);

        Assert.Collection(
            completedDailyResponse.AttributeRewards,
            primaryReward =>
            {
                Assert.Equal(
                    AttributeType.Mind,
                    primaryReward.AttributeType);

                Assert.Equal(
                    21,
                    primaryReward.XpAmount);
            },
            secondaryReward =>
            {
                Assert.Equal(
                    AttributeType.Focus,
                    secondaryReward.AttributeType);

                Assert.Equal(
                    9,
                    secondaryReward.XpAmount);
            });

        var incompleteDailyResponse =
            Assert.Single(
                response.TodayHabits,
                habit =>
                    habit.Id
                    == incompleteDailyHabit.Id);

        Assert.False(
            incompleteDailyResponse
                .IsCompletedToday);
    }

    [Fact]
    public async Task GetDashboardAsync_WhenHabitHasFutureConfiguration_UsesConfigurationEffectiveToday()
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
                isActive: true,
                name: "Scheduled configuration habit",
                category:
                    HabitCategory.GeneralGrowth,
                difficulty:
                    HabitDifficulty.Medium);

        var currentConfiguration =
            Assert.Single(
                habit.HabitConfigurationVersions);

        currentConfiguration
            .EffectiveToDateExclusive =
                new DateOnly(2026, 7, 27);

        habit.HabitConfigurationVersions.Add(
            new HabitConfigurationVersion
            {
                HabitId = habit.Id,
                VersionNumber = 2,
                Category =
                    HabitCategory.FitnessAndMovement,
                FrequencyType =
                    HabitFrequencyType.Weekly,
                TargetCount = 3,
                Difficulty =
                    HabitDifficulty.Elite,
                EffectiveFromDate =
                    new DateOnly(2026, 7, 27),
                CreatedAtUtc =
                    FixedUtcNow.UtcDateTime
            });

        dbContext.Habits.Add(habit);

        await dbContext.SaveChangesAsync();

        var dashboardService =
            CreateDashboardService(dbContext);

        var response =
            await dashboardService.GetDashboardAsync(
                userId);

        var dashboardHabit =
            Assert.Single(
                response.TodayHabits);

        Assert.Equal(
            HabitCategory.GeneralGrowth,
            dashboardHabit.Category);

        Assert.Equal(
            HabitFrequencyType.Daily,
            dashboardHabit.FrequencyType);

        Assert.Equal(
            1,
            dashboardHabit.TargetCount);

        Assert.Equal(
            HabitDifficulty.Medium,
            dashboardHabit.Difficulty);
    }

    [Fact]
    public async Task GetDashboardAsync_WhenUserHasAttributeProgress_ReturnsAllOwnedAttributes()
    {
        await using var dbContext =
            CreateDbContext();

        var userId = Guid.CreateVersion7();
        var otherUserId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        dbContext.UserAttributes.AddRange(
            new UserAttribute
            {
                UserId = userId,
                AttributeType =
                    AttributeType.Discipline,
                CurrentXp = 99,
                UpdatedAtUtc =
                    FixedUtcNow.UtcDateTime
            },
            new UserAttribute
            {
                UserId = userId,
                AttributeType =
                    AttributeType.Fitness,
                CurrentXp = 225,
                UpdatedAtUtc =
                    FixedUtcNow.UtcDateTime
            },
            new UserAttribute
            {
                UserId = otherUserId,
                AttributeType =
                    AttributeType.Mind,
                CurrentXp = 999,
                UpdatedAtUtc =
                    FixedUtcNow.UtcDateTime
            });

        await dbContext.SaveChangesAsync();

        var dashboardService =
            CreateDashboardService(dbContext);

        var response =
            await dashboardService.GetDashboardAsync(
                userId);

        Assert.Equal(
            Enum.GetValues<AttributeType>(),
            response.Attributes
                .Select(attribute =>
                    attribute.AttributeType)
                .ToArray());

        var discipline =
            Assert.Single(
                response.Attributes,
                attribute =>
                    attribute.AttributeType
                    == AttributeType.Discipline);

        Assert.Equal(
            99,
            discipline.CurrentXp);

        Assert.Equal(
            1,
            discipline.Level);

        var fitness =
            Assert.Single(
                response.Attributes,
                attribute =>
                    attribute.AttributeType
                    == AttributeType.Fitness);

        Assert.Equal(
            225,
            fitness.CurrentXp);

        Assert.Equal(
            3,
            fitness.Level);

        var mind =
            Assert.Single(
                response.Attributes,
                attribute =>
                    attribute.AttributeType
                    == AttributeType.Mind);

        Assert.Equal(
            0,
            mind.CurrentXp);
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
            Assert.Single(
                response.HabitStreaks);

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

        var dashboardHabit =
            Assert.Single(
                response.TodayHabits);

        Assert.Equal(
            2,
            dashboardHabit.CurrentStreak);

        Assert.Equal(
            2,
            dashboardHabit.LongestStreak);
    }

    private static DashboardService CreateDashboardService(
        AppDbContext dbContext)
    {
        var xpService =
            new XpService();

        return new DashboardService(
            dbContext,
            xpService,
            new StreakService(),
            new AttributeService(
                dbContext,
                xpService),
            new FixedTimeProvider(
                FixedUtcNow));
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
        bool isActive,
        string name = "Dashboard test habit",
        HabitCategory category =
            HabitCategory.GeneralGrowth,
        HabitDifficulty difficulty =
            HabitDifficulty.Medium)
    {
        var createdAtUtc =
            FixedUtcNow.UtcDateTime.AddDays(-7);

        var targetCount =
            frequencyType
                == HabitFrequencyType.Daily
                    ? 1
                    : 3;

        var habit =
            new Habit
            {
                UserId = userId,
                Name = name,
                Category = category,
                FrequencyType = frequencyType,
                TargetCount = targetCount,
                Difficulty = difficulty,
                IsActive = isActive,
                CreatedAtUtc = createdAtUtc,
                UpdatedAtUtc = createdAtUtc
            };

        habit.HabitConfigurationVersions.Add(
            new HabitConfigurationVersion
            {
                HabitId = habit.Id,
                VersionNumber = 1,
                Category = category,
                FrequencyType = frequencyType,
                TargetCount = targetCount,
                Difficulty = difficulty,
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
