using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.DTOs;
using HabitTracker.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Tests.Services;

public sealed class CompletionXpTests
{
    [Fact]
    public async Task CompleteHabitAsync_WhenRewardsExist_AwardsAttributeXpAndCreatesTransactions()
    {
        await using var dbContext = CreateDbContext();

        var utcNow =
            new DateTimeOffset(
                2026,
                7,
                20,
                2,
                30,
                0,
                TimeSpan.Zero);

        var userId = Guid.CreateVersion7();
        var habitId = Guid.CreateVersion7();

        AddUserAndSettings(
            dbContext,
            userId,
            utcNow.UtcDateTime);

        var habit =
            CreateHabit(
                userId,
                habitId,
                utcNow.UtcDateTime);

        habit.HabitAttributeRewards.Add(
            new HabitAttributeReward
            {
                HabitId = habitId,
                AttributeType =
                    AttributeType.Fitness,
                XpAmount = 14
            });

        habit.HabitAttributeRewards.Add(
            new HabitAttributeReward
            {
                HabitId = habitId,
                AttributeType =
                    AttributeType.Discipline,
                XpAmount = 6
            });

        dbContext.Habits.Add(habit);

        dbContext.UserAttributes.Add(
            new UserAttribute
            {
                UserId = userId,
                AttributeType =
                    AttributeType.Fitness,
                CurrentXp = 10,
                UpdatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1)
            });

        await dbContext.SaveChangesAsync();

        var completionService =
            CreateCompletionService(
                dbContext,
                utcNow);

        var response =
            await completionService.CompleteHabitAsync(
                userId,
                habitId,
                new CompleteHabitRequest());

        Assert.NotNull(response);

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
                    utcNow.UtcDateTime,
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

        var utcNow =
            new DateTimeOffset(
                2026,
                7,
                20,
                2,
                30,
                0,
                TimeSpan.Zero);

        var userId = Guid.CreateVersion7();
        var habitId = Guid.CreateVersion7();

        AddUserAndSettings(
            dbContext,
            userId,
            utcNow.UtcDateTime);

        var habit =
            CreateHabit(
                userId,
                habitId,
                utcNow.UtcDateTime);

        var completion =
            new HabitCompletion
            {
                UserId = userId,
                HabitId = habitId,
                CompletedDate =
                    new DateOnly(2026, 7, 19),
                CompletedAtUtc =
                    utcNow.UtcDateTime.AddHours(-1)
            };

        dbContext.Habits.Add(habit);
        dbContext.HabitCompletions.Add(completion);

        dbContext.UserAttributes.AddRange(
            new UserAttribute
            {
                UserId = userId,
                AttributeType =
                    AttributeType.Fitness,
                CurrentXp = 24,
                UpdatedAtUtc =
                    utcNow.UtcDateTime.AddHours(-1)
            },
            new UserAttribute
            {
                UserId = userId,
                AttributeType =
                    AttributeType.Discipline,
                CurrentXp = 6,
                UpdatedAtUtc =
                    utcNow.UtcDateTime.AddHours(-1)
            });

        dbContext.XpTransactions.AddRange(
            new XpTransaction
            {
                UserId = userId,
                HabitCompletionId = completion.Id,
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
                HabitCompletionId = completion.Id,
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
                utcNow);

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
                    utcNow.UtcDateTime,
                    attribute.UpdatedAtUtc));
    }

    private static CompletionService CreateCompletionService(
        AppDbContext dbContext,
        DateTimeOffset utcNow)
    {
        return new CompletionService(
            dbContext,
            new FixedTimeProvider(utcNow),
            new AttributeService(dbContext),
            new XpService());
    }

    private static void AddUserAndSettings(
        AppDbContext dbContext,
        Guid userId,
        DateTime timestampUtc)
    {
        dbContext.Users.Add(
            new User
            {
                Id = userId,
                Email = "fred@example.com",
                NormalizedEmail =
                    "FRED@EXAMPLE.COM",
                Username = "fred",
                NormalizedUsername = "FRED",
                PasswordHash =
                    "test-password-hash",
                CreatedAtUtc =
                    timestampUtc.AddDays(-1)
            });

        dbContext.UserSettings.Add(
            new UserSettings
            {
                UserId = userId,
                DisplayName = "Fred",
                TimeZone = "America/Toronto",
                CreatedAtUtc =
                    timestampUtc.AddDays(-1),
                UpdatedAtUtc =
                    timestampUtc.AddDays(-1)
            });
    }

    private static Habit CreateHabit(
        Guid userId,
        Guid habitId,
        DateTime timestampUtc)
    {
        return new Habit
        {
            Id = habitId,
            UserId = userId,
            Name = "Go to gym",
            Category =
                HabitCategory.FitnessAndMovement,
            FrequencyType =
                HabitFrequencyType.Daily,
            TargetCount = 1,
            Difficulty =
                HabitDifficulty.Medium,
            IsActive = true,
            CreatedAtUtc =
                timestampUtc.AddDays(-1),
            UpdatedAtUtc =
                timestampUtc.AddDays(-1)
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
