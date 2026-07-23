using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Tests.Services;

public sealed class AttributeServiceTests
{
    [Fact]
    public async Task GetUserAttributesAsync_ReturnsAllSupportedAttributes()
    {
        await using var dbContext =
            CreateDbContext();

        var userId =
            Guid.CreateVersion7();

        dbContext.UserAttributes.AddRange(
            new UserAttribute
            {
                UserId = userId,
                AttributeType =
                    AttributeType.Discipline,
                CurrentXp = 99,
                UpdatedAtUtc = DateTime.UtcNow
            },
            new UserAttribute
            {
                UserId = userId,
                AttributeType =
                    AttributeType.Fitness,
                CurrentXp = 225,
                UpdatedAtUtc = DateTime.UtcNow
            });

        await dbContext.SaveChangesAsync();

        var service =
            CreateService(dbContext);

        var attributes =
            await service.GetUserAttributesAsync(
                userId);

        Assert.Equal(
            Enum.GetValues<AttributeType>(),
            attributes
                .Select(attribute =>
                    attribute.AttributeType)
                .ToArray());

        var discipline =
            Assert.Single(
                attributes,
                attribute =>
                    attribute.AttributeType
                    == AttributeType.Discipline);

        Assert.Equal(99, discipline.CurrentXp);
        Assert.Equal(1, discipline.Level);
        Assert.Equal(
            99,
            discipline.XpIntoCurrentLevel);
        Assert.Equal(
            100,
            discipline.XpNeededForNextLevel);

        var fitness =
            Assert.Single(
                attributes,
                attribute =>
                    attribute.AttributeType
                    == AttributeType.Fitness);

        Assert.Equal(225, fitness.CurrentXp);
        Assert.Equal(3, fitness.Level);
        Assert.Equal(
            0,
            fitness.XpIntoCurrentLevel);
        Assert.Equal(
            150,
            fitness.XpNeededForNextLevel);

        var vitality =
            Assert.Single(
                attributes,
                attribute =>
                    attribute.AttributeType
                    == AttributeType.Vitality);

        Assert.Equal(0, vitality.CurrentXp);
        Assert.Equal(1, vitality.Level);
        Assert.Equal(
            0,
            vitality.XpIntoCurrentLevel);
        Assert.Equal(
            100,
            vitality.XpNeededForNextLevel);
    }

    [Fact]
    public async Task GetUserAttributesAsync_DoesNotReturnAnotherUsersXp()
    {
        await using var dbContext =
            CreateDbContext();

        var requestingUserId =
            Guid.CreateVersion7();

        var otherUserId =
            Guid.CreateVersion7();

        dbContext.UserAttributes.Add(
            new UserAttribute
            {
                UserId = otherUserId,
                AttributeType =
                    AttributeType.Fitness,
                CurrentXp = 999,
                UpdatedAtUtc = DateTime.UtcNow
            });

        await dbContext.SaveChangesAsync();

        var service =
            CreateService(dbContext);

        var attributes =
            await service.GetUserAttributesAsync(
                requestingUserId);

        Assert.All(
            attributes,
            attribute =>
                Assert.Equal(
                    0,
                    attribute.CurrentXp));
    }

    [Fact]
    public async Task GetAttributeOverviewAsync_ReturnsBalanceSummariesQueueAndOwnedRecentXp()
    {
        await using var dbContext =
            CreateDbContext();

        var userId =
            Guid.CreateVersion7();

        var otherUserId =
            Guid.CreateVersion7();

        AddAttributeProgress(
            dbContext,
            userId);

        var baseTimestampUtc =
            new DateTime(
                2026,
                7,
                23,
                12,
                0,
                0,
                DateTimeKind.Utc);

        AddXpHistory(
            dbContext,
            userId,
            "Read C# textbook",
            AttributeType.Mind,
            30,
            "Habit completion",
            baseTimestampUtc);

        AddXpHistory(
            dbContext,
            userId,
            "Meditate",
            AttributeType.Focus,
            -25,
            "Habit completion undo",
            baseTimestampUtc.AddMinutes(1));

        AddXpHistory(
            dbContext,
            otherUserId,
            "Private habit",
            AttributeType.Fitness,
            999,
            "Habit completion",
            baseTimestampUtc.AddMinutes(2));

        await dbContext.SaveChangesAsync();

        var service =
            CreateService(dbContext);

        var overview =
            await service.GetAttributeOverviewAsync(
                userId);

        Assert.Equal(
            10060,
            overview.TotalAttributeXp);

        Assert.Equal(
            68,
            overview.BalanceScore);

        Assert.NotNull(
            overview.StrongestAttribute);

        Assert.Equal(
            AttributeType.Discipline,
            overview.StrongestAttribute
                .AttributeType);

        Assert.NotNull(
            overview.NeedsFocusAttribute);

        Assert.Equal(
            AttributeType.Social,
            overview.NeedsFocusAttribute
                .AttributeType);

        Assert.Collection(
            overview.ClosestToLevelUp,
            first =>
            {
                Assert.Equal(
                    AttributeType.Purpose,
                    first.AttributeType);

                Assert.Equal(
                    80,
                    first.XpRemaining);
            },
            second =>
            {
                Assert.Equal(
                    AttributeType.Vitality,
                    second.AttributeType);

                Assert.Equal(
                    85,
                    second.XpRemaining);
            },
            third =>
            {
                Assert.Equal(
                    AttributeType.Mind,
                    third.AttributeType);

                Assert.Equal(
                    90,
                    third.XpRemaining);
            });

        Assert.Collection(
            overview.RecentXpTransactions,
            newest =>
            {
                Assert.Equal(
                    "Meditate",
                    newest.HabitName);

                Assert.Equal(
                    -25,
                    newest.Amount);

                Assert.Equal(
                    "Habit completion undo",
                    newest.Reason);
            },
            oldest =>
            {
                Assert.Equal(
                    "Read C# textbook",
                    oldest.HabitName);

                Assert.Equal(
                    30,
                    oldest.Amount);

                Assert.Equal(
                    "Habit completion",
                    oldest.Reason);
            });
    }

    [Fact]
    public async Task GetAttributeOverviewAsync_WhenNoProgressExists_ReturnsZeroState()
    {
        await using var dbContext =
            CreateDbContext();

        var service =
            CreateService(dbContext);

        var overview =
            await service.GetAttributeOverviewAsync(
                Guid.CreateVersion7());

        Assert.Equal(
            8,
            overview.Attributes.Count);

        Assert.Equal(
            0,
            overview.TotalAttributeXp);

        Assert.Equal(
            0,
            overview.BalanceScore);

        Assert.Null(
            overview.StrongestAttribute);

        Assert.Null(
            overview.NeedsFocusAttribute);

        Assert.Collection(
            overview.ClosestToLevelUp,
            first =>
            {
                Assert.Equal(
                    AttributeType.Discipline,
                    first.AttributeType);

                Assert.Equal(
                    100,
                    first.XpRemaining);
            },
            second =>
            {
                Assert.Equal(
                    AttributeType.Fitness,
                    second.AttributeType);

                Assert.Equal(
                    100,
                    second.XpRemaining);
            },
            third =>
            {
                Assert.Equal(
                    AttributeType.Vitality,
                    third.AttributeType);

                Assert.Equal(
                    100,
                    third.XpRemaining);
            });

        Assert.Empty(
            overview.RecentXpTransactions);
    }

    private static AttributeService CreateService(
        AppDbContext dbContext)
    {
        return new AttributeService(
            dbContext,
            new XpService());
    }

    private static void AddAttributeProgress(
        AppDbContext dbContext,
        Guid userId)
    {
        var currentXpByAttribute =
            new Dictionary<AttributeType, int>
            {
                [AttributeType.Discipline] = 1860,
                [AttributeType.Fitness] = 1300,
                [AttributeType.Vitality] = 890,
                [AttributeType.Focus] = 1250,
                [AttributeType.Mind] = 1710,
                [AttributeType.Resilience] = 980,
                [AttributeType.Social] = 650,
                [AttributeType.Purpose] = 1420
            };

        dbContext.UserAttributes.AddRange(
            currentXpByAttribute.Select(
                item =>
                    new UserAttribute
                    {
                        UserId = userId,
                        AttributeType = item.Key,
                        CurrentXp = item.Value,
                        UpdatedAtUtc =
                            DateTime.UtcNow
                    }));
    }

    private static void AddXpHistory(
        AppDbContext dbContext,
        Guid userId,
        string habitName,
        AttributeType attributeType,
        int amount,
        string reason,
        DateTime createdAtUtc)
    {
        var habit =
            new Habit
            {
                UserId = userId,
                Name = habitName,
                Category =
                    HabitCategory.GeneralGrowth,
                FrequencyType =
                    HabitFrequencyType.Daily,
                TargetCount = 1,
                Difficulty =
                    HabitDifficulty.Medium,
                IsActive = true,
                CreatedAtUtc =
                    createdAtUtc.AddDays(-1),
                UpdatedAtUtc =
                    createdAtUtc.AddDays(-1)
            };

        var configuration =
            new HabitConfigurationVersion
            {
                HabitId = habit.Id,
                VersionNumber = 1,
                Category = habit.Category,
                FrequencyType =
                    habit.FrequencyType,
                TargetCount =
                    habit.TargetCount,
                Difficulty =
                    habit.Difficulty,
                EffectiveFromDate =
                    DateOnly.FromDateTime(
                        createdAtUtc.AddDays(-1)),
                CreatedAtUtc =
                    createdAtUtc.AddDays(-1)
            };

        habit.HabitConfigurationVersions.Add(
            configuration);

        var completion =
            new HabitCompletion
            {
                UserId = userId,
                HabitId = habit.Id,
                HabitConfigurationVersionId =
                    configuration.Id,
                CompletedDate =
                    DateOnly.FromDateTime(
                        createdAtUtc),
                CompletedAtUtc =
                    createdAtUtc
            };

        habit.HabitCompletions.Add(
            completion);

        completion.XpTransactions.Add(
            new XpTransaction
            {
                UserId = userId,
                HabitCompletionId =
                    completion.Id,
                AttributeType =
                    attributeType,
                Amount = amount,
                Reason = reason,
                CreatedAtUtc =
                    createdAtUtc
            });

        dbContext.Habits.Add(
            habit);
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
}
