using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.DTOs;
using HabitTracker.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Tests.Services;

public sealed class CompletionXpTests
{
    private static readonly DateTimeOffset UtcNow =
        new(2026, 7, 20, 2, 30, 0, TimeSpan.Zero);

    private static readonly DateOnly CurrentLocalDate =
        new(2026, 7, 19);

    [Fact]
    public async Task CompleteHabitAsync_WhenConfigurationIsEffective_AwardsAttributeXpAndCreatesTransactions()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();
        var habitId = Guid.CreateVersion7();

        AddUserAndSettings(
            dbContext,
            userId,
            UtcNow.UtcDateTime);

        var (habit, configuration) =
            CreateHabitWithConfiguration(
                userId,
                habitId,
                UtcNow.UtcDateTime);

        dbContext.Habits.Add(habit);

        dbContext.UserAttributes.Add(
            new UserAttribute
            {
                UserId = userId,
                AttributeType =
                    AttributeType.Fitness,
                CurrentXp = 10,
                UpdatedAtUtc =
                    UtcNow.UtcDateTime.AddDays(-1)
            });

        await dbContext.SaveChangesAsync();

        var completionService =
            CreateCompletionService(
                dbContext,
                UtcNow);

        var response =
            await completionService.CompleteHabitAsync(
                userId,
                habitId,
                new CompleteHabitRequest());

        Assert.NotNull(response);

        var savedCompletion =
            Assert.Single(
                dbContext.HabitCompletions);

        Assert.Equal(
            configuration.Id,
            savedCompletion.HabitConfigurationVersionId);

        Assert.Collection(
            response.Rewards,
            primaryReward =>
            {
                Assert.Equal(
                    AttributeType.Fitness,
                    primaryReward.AttributeType);
                Assert.Equal(
                    14,
                    primaryReward.XpAmount);
            },
            secondaryReward =>
            {
                Assert.Equal(
                    AttributeType.Discipline,
                    secondaryReward.AttributeType);
                Assert.Equal(
                    6,
                    secondaryReward.XpAmount);
            });

        var attributes =
            dbContext.UserAttributes
                .Where(attribute =>
                    attribute.UserId == userId)
                .ToDictionary(
                    attribute =>
                        attribute.AttributeType,
                    attribute =>
                        attribute.CurrentXp);

        Assert.Equal(
            24,
            attributes[AttributeType.Fitness]);

        Assert.Equal(
            6,
            attributes[AttributeType.Discipline]);

        var transactions =
            dbContext.XpTransactions
                .Where(transaction =>
                    transaction.UserId == userId)
                .ToList();

        Assert.Equal(2, transactions.Count);

        Assert.All(
            transactions,
            transaction =>
            {
                Assert.Equal(
                    response.Completion.Id,
                    transaction.HabitCompletionId);

                Assert.Equal(
                    "Habit completion",
                    transaction.Reason);

                Assert.Equal(
                    UtcNow.UtcDateTime,
                    transaction.CreatedAtUtc);
            });

        Assert.Contains(
            transactions,
            transaction =>
                transaction.AttributeType
                    == AttributeType.Fitness
                && transaction.Amount == 14);

        Assert.Contains(
            transactions,
            transaction =>
                transaction.AttributeType
                    == AttributeType.Discipline
                && transaction.Amount == 6);
    }

    [Fact]
    public async Task UndoTodayAsync_WhenXpWasAwarded_ReversesXpAndDeletesTransactions()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();
        var habitId = Guid.CreateVersion7();

        AddUserAndSettings(
            dbContext,
            userId,
            UtcNow.UtcDateTime);

        var (habit, configuration) =
            CreateHabitWithConfiguration(
                userId,
                habitId,
                UtcNow.UtcDateTime);

        var completion =
            new HabitCompletion
            {
                UserId = userId,
                HabitId = habitId,
                Habit = habit,
                HabitConfigurationVersionId =
                    configuration.Id,
                HabitConfigurationVersion =
                    configuration,
                CompletedDate = CurrentLocalDate,
                CompletedAtUtc =
                    UtcNow.UtcDateTime.AddHours(-1)
            };

        dbContext.Habits.Add(habit);
        dbContext.HabitCompletions.Add(
            completion);

        dbContext.UserAttributes.AddRange(
            new UserAttribute
            {
                UserId = userId,
                AttributeType =
                    AttributeType.Fitness,
                CurrentXp = 24,
                UpdatedAtUtc =
                    UtcNow.UtcDateTime.AddHours(-1)
            },
            new UserAttribute
            {
                UserId = userId,
                AttributeType =
                    AttributeType.Discipline,
                CurrentXp = 6,
                UpdatedAtUtc =
                    UtcNow.UtcDateTime.AddHours(-1)
            });

        dbContext.XpTransactions.AddRange(
            new XpTransaction
            {
                UserId = userId,
                HabitCompletionId =
                    completion.Id,
                AttributeType =
                    AttributeType.Fitness,
                Amount = 14,
                Reason = "Habit completion",
                CreatedAtUtc =
                    completion.CompletedAtUtc
            },
            new XpTransaction
            {
                UserId = userId,
                HabitCompletionId =
                    completion.Id,
                AttributeType =
                    AttributeType.Discipline,
                Amount = 6,
                Reason = "Habit completion",
                CreatedAtUtc =
                    completion.CompletedAtUtc
            });

        await dbContext.SaveChangesAsync();

        var completionService =
            CreateCompletionService(
                dbContext,
                UtcNow);

        var wasRemoved =
            await completionService.UndoTodayAsync(
                userId,
                habitId);

        Assert.True(wasRemoved);
        Assert.Empty(dbContext.HabitCompletions);
        Assert.Empty(dbContext.XpTransactions);

        var attributes =
            dbContext.UserAttributes
                .Where(attribute =>
                    attribute.UserId == userId)
                .ToDictionary(
                    attribute =>
                        attribute.AttributeType,
                    attribute =>
                        attribute.CurrentXp);

        Assert.Equal(
            10,
            attributes[AttributeType.Fitness]);

        Assert.Equal(
            0,
            attributes[AttributeType.Discipline]);

        Assert.All(
            dbContext.UserAttributes,
            attribute =>
                Assert.Equal(
                    UtcNow.UtcDateTime,
                    attribute.UpdatedAtUtc));
    }

    private static CompletionService
        CreateCompletionService(
            AppDbContext dbContext,
            DateTimeOffset utcNow)
    {
        var xpService = new XpService();

        return new CompletionService(
            dbContext,
            new FixedTimeProvider(utcNow),
            new AttributeService(
                dbContext,
                xpService),
            xpService);
    }

    private static void AddUserAndSettings(
        AppDbContext dbContext,
        Guid userId,
        DateTime timestampUtc)
    {
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
            CreatedAtUtc =
                timestampUtc.AddDays(-1)
        };

        var settings = new UserSettings
        {
            UserId = userId,
            DisplayName = "Test User",
            TimeZone = "America/Toronto",
            CreatedAtUtc =
                timestampUtc.AddDays(-1),
            UpdatedAtUtc =
                timestampUtc.AddDays(-1),
            User = user
        };

        user.UserSettings = settings;
        dbContext.Users.Add(user);
    }

    private static (
        Habit Habit,
        HabitConfigurationVersion Configuration)
        CreateHabitWithConfiguration(
            Guid userId,
            Guid habitId,
            DateTime timestampUtc)
    {
        var habit = new Habit
        {
            Id = habitId,
            UserId = userId,
            Name = "Go to gym",
            Category =
                HabitCategory.GeneralGrowth,
            FrequencyType =
                HabitFrequencyType.Daily,
            TargetCount = 1,
            Difficulty =
                HabitDifficulty.Easy,
            IsActive = true,
            CreatedAtUtc =
                timestampUtc.AddDays(-1),
            UpdatedAtUtc =
                timestampUtc.AddDays(-1)
        };

        var configuration =
            new HabitConfigurationVersion
            {
                HabitId = habitId,
                VersionNumber = 1,
                Category =
                    HabitCategory.FitnessAndMovement,
                FrequencyType =
                    HabitFrequencyType.Daily,
                TargetCount = 1,
                Difficulty =
                    HabitDifficulty.Medium,
                EffectiveFromDate =
                    CurrentLocalDate.AddDays(-1),
                CreatedAtUtc =
                    timestampUtc.AddDays(-1)
            };

        habit.HabitConfigurationVersions.Add(
            configuration);

        return (habit, configuration);
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
