using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.DTOs;
using HabitTracker.Api.Exceptions;
using HabitTracker.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Tests.Services;

public sealed class HabitServiceTests
{
    private static readonly DateTime TestUtcNow =
        new(
            2026,
            7,
            19,
            12,
            0,
            0,
            DateTimeKind.Utc);

    [Fact]
    public async Task CreateHabitAsync_WhenRequestIsValid_CreatesAndReturnsHabit()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        await dbContext.SaveChangesAsync();

        var habitService = CreateHabitService(dbContext);

        var expectedCreatedAtUtc =
            new DateTime(
                2026,
                7,
                20,
                2,
                30,
                0,
                DateTimeKind.Utc);

        var request = new CreateHabitRequest
        {
            Name = "  Go to gym  ",
            Description = "  Complete a planned workout.  ",
            Category = HabitCategory.FitnessAndMovement,
            FrequencyType = HabitFrequencyType.Weekly,
            TargetCount = 3,
            Difficulty = HabitDifficulty.Elite
        };

        var response =
            await habitService.CreateHabitAsync(
                userId,
                request);

        var savedHabit =
            Assert.Single(dbContext.Habits);

        var savedConfiguration =
            Assert.Single(
                dbContext.HabitConfigurationVersions);

        Assert.Equal(
            savedHabit.Id,
            savedConfiguration.HabitId);

        Assert.Equal(
            1,
            savedConfiguration.VersionNumber);

        Assert.Equal(
            savedHabit.Category,
            savedConfiguration.Category);

        Assert.Equal(
            savedHabit.FrequencyType,
            savedConfiguration.FrequencyType);

        Assert.Equal(
            savedHabit.TargetCount,
            savedConfiguration.TargetCount);

        Assert.Equal(
            savedHabit.Difficulty,
            savedConfiguration.Difficulty);

        Assert.Equal(
            new DateOnly(2026, 7, 19),
            savedConfiguration.EffectiveFromDate);

        Assert.Null(
            savedConfiguration.EffectiveToDateExclusive);

        Assert.Equal(
            expectedCreatedAtUtc,
            savedConfiguration.CreatedAtUtc);

        Assert.NotEqual(Guid.Empty, savedHabit.Id);
        Assert.Equal(userId, savedHabit.UserId);
        Assert.Equal("Go to gym", savedHabit.Name);
        Assert.Equal(
            "Complete a planned workout.",
            savedHabit.Description);
        Assert.Equal(
            HabitCategory.FitnessAndMovement,
            savedHabit.Category);
        Assert.Equal(
            HabitFrequencyType.Weekly,
            savedHabit.FrequencyType);
        Assert.Equal(3, savedHabit.TargetCount);
        Assert.Equal(
            HabitDifficulty.Elite,
            savedHabit.Difficulty);
        Assert.True(savedHabit.IsActive);

        var savedRewards =
            dbContext.HabitAttributeRewards
                .Where(reward =>
                    reward.HabitId == savedHabit.Id)
                .ToDictionary(
                    reward => reward.AttributeType,
                    reward => reward.XpAmount);

        Assert.Equal(2, savedRewards.Count);

        Assert.Equal(
            35,
            savedRewards[AttributeType.Fitness]);

        Assert.Equal(
            15,
            savedRewards[AttributeType.Discipline]);

        Assert.Equal(
            expectedCreatedAtUtc,
            savedHabit.CreatedAtUtc);

        Assert.Equal(
            savedHabit.CreatedAtUtc,
            savedHabit.UpdatedAtUtc);

        Assert.Equal(savedHabit.Id, response.Id);
        Assert.Equal(savedHabit.Name, response.Name);
        Assert.Equal(
            savedHabit.Description,
            response.Description);
        Assert.Equal(savedHabit.Category, response.Category);
        Assert.Equal(
            savedHabit.FrequencyType,
            response.FrequencyType);
        Assert.Equal(
            savedHabit.TargetCount,
            response.TargetCount);
        Assert.Equal(
            savedHabit.Difficulty,
            response.Difficulty);
        Assert.Collection(
            response.AttributeRewards,
            primaryReward =>
            {
                Assert.Equal(
                    AttributeType.Fitness,
                    primaryReward.AttributeType);

                Assert.Equal(
                    35,
                    primaryReward.XpAmount);
            },
            secondaryReward =>
            {
                Assert.Equal(
                    AttributeType.Discipline,
                    secondaryReward.AttributeType);

                Assert.Equal(
                    15,
                    secondaryReward.XpAmount);
            });
        Assert.Equal(savedHabit.IsActive, response.IsActive);
        Assert.False(response.IsCompletedToday);
        Assert.Equal(
            savedHabit.CreatedAtUtc,
            response.CreatedAtUtc);
        Assert.Equal(
            savedHabit.UpdatedAtUtc,
            response.UpdatedAtUtc);
    }

    [Fact]
    public async Task CreateHabitAsync_WhenDescriptionIsBlank_StoresNull()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        await dbContext.SaveChangesAsync();

        var habitService = CreateHabitService(dbContext);

        var request = CreateValidRequest();

        request.Description = "   ";

        var response =
            await habitService.CreateHabitAsync(
                userId,
                request);

        var savedHabit =
            Assert.Single(dbContext.Habits);

        Assert.Single(
            dbContext.HabitConfigurationVersions);
        Assert.Null(savedHabit.Description);
        Assert.Null(response.Description);
    }

    [Fact]
    public async Task CreateHabitAsync_WhenNameIsWhitespace_ThrowsInvalidHabitNameException()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        await dbContext.SaveChangesAsync();

        var habitService = CreateHabitService(dbContext);

        var request = CreateValidRequest();

        request.Name = "   ";

        await Assert.ThrowsAsync<InvalidHabitNameException>(
            () =>
                habitService.CreateHabitAsync(
                    userId,
                    request));

        Assert.Empty(dbContext.Habits);
        Assert.Empty(
            dbContext.HabitConfigurationVersions);
    }

    [Theory]
    [InlineData(HabitFrequencyType.Daily, 2)]
    [InlineData(HabitFrequencyType.Weekly, 0)]
    [InlineData(HabitFrequencyType.Weekly, 8)]
    [InlineData((HabitFrequencyType)999, 1)]
    public async Task CreateHabitAsync_WhenTargetConfigurationIsInvalid_ThrowsInvalidHabitTargetCountException(
        HabitFrequencyType frequencyType,
        int targetCount)
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        await dbContext.SaveChangesAsync();

        var habitService = CreateHabitService(dbContext);

        var request = CreateValidRequest();

        request.FrequencyType = frequencyType;
        request.TargetCount = targetCount;

        await Assert.ThrowsAsync<InvalidHabitTargetCountException>(
            () =>
                habitService.CreateHabitAsync(
                    userId,
                    request));

        Assert.Empty(dbContext.Habits);
        Assert.Empty(
            dbContext.HabitConfigurationVersions);
    }

    [Fact]
    public async Task GetUserHabitsAsync_WhenInactiveHabitsAreExcluded_ReturnsOnlyOwnedActiveHabitsNewestFirst()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();
        AddUserWithSettings(
            dbContext,
            userId);
        var otherUserId = Guid.CreateVersion7();
        var baseTimestampUtc = TestUtcNow.AddDays(-1);

        var olderActiveHabit =
            CreateHabit(
                userId,
                "Older active habit",
                baseTimestampUtc,
                isActive: true);

        var newerActiveHabit =
            CreateHabit(
                userId,
                "Newer active habit",
                baseTimestampUtc.AddHours(1),
                isActive: true);

        var inactiveHabit =
            CreateHabit(
                userId,
                "Inactive habit",
                baseTimestampUtc.AddHours(2),
                isActive: false);

        var otherUserHabit =
            CreateHabit(
                otherUserId,
                "Another user's habit",
                baseTimestampUtc.AddHours(3),
                isActive: true);

        dbContext.Habits.AddRange(
            olderActiveHabit,
            newerActiveHabit,
            inactiveHabit,
            otherUserHabit);

        await dbContext.SaveChangesAsync();

        var habitService = CreateHabitService(dbContext);

        var response =
            await habitService.GetUserHabitsAsync(
                userId);

        Assert.Collection(
            response,
            firstHabit =>
                Assert.Equal(
                    newerActiveHabit.Id,
                    firstHabit.Id),
            secondHabit =>
                Assert.Equal(
                    olderActiveHabit.Id,
                    secondHabit.Id));
    }

    [Fact]
    public async Task GetUserHabitsAsync_WhenInactiveHabitsAreIncluded_ReturnsOwnedActiveHabitsBeforeInactiveHabits()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();
        AddUserWithSettings(
            dbContext,
            userId);
        var baseTimestampUtc = TestUtcNow.AddDays(-1);

        var olderActiveHabit =
            CreateHabit(
                userId,
                "Older active habit",
                baseTimestampUtc,
                isActive: true);

        var newerActiveHabit =
            CreateHabit(
                userId,
                "Newer active habit",
                baseTimestampUtc.AddHours(1),
                isActive: true);

        var olderInactiveHabit =
            CreateHabit(
                userId,
                "Older inactive habit",
                baseTimestampUtc.AddHours(2),
                isActive: false);

        var newerInactiveHabit =
            CreateHabit(
                userId,
                "Newer inactive habit",
                baseTimestampUtc.AddHours(3),
                isActive: false);

        dbContext.Habits.AddRange(
            olderActiveHabit,
            newerActiveHabit,
            olderInactiveHabit,
            newerInactiveHabit);

        await dbContext.SaveChangesAsync();

        var habitService = CreateHabitService(dbContext);

        var response =
            await habitService.GetUserHabitsAsync(
                userId,
                includeInactive: true);

        Assert.Collection(
            response,
            firstHabit =>
                Assert.Equal(
                    newerActiveHabit.Id,
                    firstHabit.Id),
            secondHabit =>
                Assert.Equal(
                    olderActiveHabit.Id,
                    secondHabit.Id),
            thirdHabit =>
                Assert.Equal(
                    newerInactiveHabit.Id,
                    thirdHabit.Id),
            fourthHabit =>
                Assert.Equal(
                    olderInactiveHabit.Id,
                    fourthHabit.Id));
    }

    [Fact]
    public async Task GetUserHabitAsync_WhenHabitBelongsToUser_ReturnsHabit()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        var habit =
            CreateHabit(
                userId,
                "Read C# textbook",
                TestUtcNow,
                isActive: false);

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var habitService = CreateHabitService(dbContext);

        var response =
            await habitService.GetUserHabitAsync(
                userId,
                habit.Id);

        Assert.NotNull(response);
        Assert.Equal(habit.Id, response.Id);
        Assert.Equal(habit.Name, response.Name);
        Assert.False(response.IsActive);
    }

    [Fact]
    public async Task GetUserHabitAsync_WhenHabitBelongsToAnotherUser_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();

        var ownerUserId = Guid.CreateVersion7();
        var requestingUserId = Guid.CreateVersion7();

        var habit =
            CreateHabit(
                ownerUserId,
                "Private habit",
                TestUtcNow,
                isActive: true);

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var habitService = CreateHabitService(dbContext);

        var response =
            await habitService.GetUserHabitAsync(
                requestingUserId,
                habit.Id);

        Assert.Null(response);
    }

    [Fact]
    public async Task GetUserHabitAsync_WhenHabitDoesNotExist_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();

        var habitService = CreateHabitService(dbContext);

        var response =
            await habitService.GetUserHabitAsync(
                Guid.CreateVersion7(),
                Guid.CreateVersion7());

        Assert.Null(response);
    }

    [Fact]
    public async Task GetUserHabitsAsync_WhenOneHabitIsCompletedToday_ReturnsCorrectCompletionStatus()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        var completedHabit =
            CreateHabit(
                userId,
                "Completed habit",
                TestUtcNow.AddHours(-2),
                isActive: true);

        var incompleteHabit =
            CreateHabit(
                userId,
                "Incomplete habit",
                TestUtcNow.AddHours(-1),
                isActive: true);

        dbContext.Habits.AddRange(
            completedHabit,
            incompleteHabit);

        dbContext.HabitCompletions.Add(
            new HabitCompletion
            {
                UserId = userId,
                HabitId = completedHabit.Id,
                CompletedDate =
                    new DateOnly(2026, 7, 19),
                CompletedAtUtc =
                    new DateTime(
                        2026,
                        7,
                        20,
                        1,
                        30,
                        0,
                        DateTimeKind.Utc)
            });

        await dbContext.SaveChangesAsync();

        var habitService =
            CreateHabitService(dbContext);

        var response =
            await habitService.GetUserHabitsAsync(
                userId);

        var completedHabitResponse =
            Assert.Single(
                response,
                habit =>
                    habit.Id == completedHabit.Id);

        var incompleteHabitResponse =
            Assert.Single(
                response,
                habit =>
                    habit.Id == incompleteHabit.Id);

        Assert.True(
            completedHabitResponse.IsCompletedToday);

        Assert.False(
            incompleteHabitResponse.IsCompletedToday);
    }

    [Fact]
    public async Task GetUserHabitAsync_WhenHabitIsCompletedToday_ReturnsCompletedStatus()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        var habit =
            CreateHabit(
                userId,
                "Completed habit",
                TestUtcNow.AddDays(-1),
                isActive: true);

        dbContext.Habits.Add(habit);

        dbContext.HabitCompletions.Add(
            new HabitCompletion
            {
                UserId = userId,
                HabitId = habit.Id,
                CompletedDate =
                    new DateOnly(2026, 7, 19),
                CompletedAtUtc =
                    new DateTime(
                        2026,
                        7,
                        20,
                        1,
                        30,
                        0,
                        DateTimeKind.Utc)
            });

        await dbContext.SaveChangesAsync();

        var habitService =
            CreateHabitService(dbContext);

        var response =
            await habitService.GetUserHabitAsync(
                userId,
                habit.Id);

        Assert.NotNull(response);
        Assert.Equal(habit.Id, response.Id);
        Assert.True(response.IsCompletedToday);
    }

    [Fact]
    public async Task UpdateHabitAsync_WhenHabitIsCompletedToday_ReturnsCompletedStatus()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        var habit =
            CreateHabit(
                userId,
                "Original habit",
                TestUtcNow.AddDays(-1),
                isActive: true);

        dbContext.Habits.Add(habit);

        dbContext.HabitCompletions.Add(
            new HabitCompletion
            {
                UserId = userId,
                HabitId = habit.Id,
                CompletedDate =
                    new DateOnly(2026, 7, 19),
                CompletedAtUtc =
                    new DateTime(
                        2026,
                        7,
                        20,
                        1,
                        30,
                        0,
                        DateTimeKind.Utc)
            });

        await dbContext.SaveChangesAsync();

        var habitService =
            CreateHabitService(dbContext);

        var response =
            await habitService.UpdateHabitAsync(
                userId,
                habit.Id,
                new UpdateHabitRequest
                {
                    Name = "Updated habit",
                    Description = null,
                    Category = HabitCategory.LearningAndSkills,
                    FrequencyType =
                        HabitFrequencyType.Daily,
                    TargetCount = 1,
                    Difficulty =
                        HabitDifficulty.Medium
                });

        Assert.NotNull(response);
        Assert.Equal("Updated habit", response.Name);
        Assert.True(response.IsCompletedToday);
    }

    [Fact]
    public async Task UpdateHabitAsync_WhenHabitBelongsToUser_UpdatesImmediateFieldsAndSchedulesRuleChanges()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        var createdAtUtc =
            new DateTime(
                2026,
                7,
                18,
                2,
                30,
                0,
                DateTimeKind.Utc);

        var previousUpdatedAtUtc =
            new DateTime(
                2026,
                7,
                19,
                2,
                30,
                0,
                DateTimeKind.Utc);

        var habit =
            CreateHabit(
                userId,
                "Old name",
                createdAtUtc,
                isActive: false);

        habit.Description = "Old description";
        habit.UpdatedAtUtc = previousUpdatedAtUtc;

        dbContext.Habits.Add(habit);

        await dbContext.SaveChangesAsync();

        var originalId = habit.Id;
        var originalUserId = habit.UserId;
        var originalCreatedAtUtc = habit.CreatedAtUtc;
        var originalIsActive = habit.IsActive;

        var habitService = CreateHabitService(dbContext);

        var response =
            await habitService.UpdateHabitAsync(
                userId,
                habit.Id,
                new UpdateHabitRequest
                {
                    Name = "  Updated habit  ",
                    Description =
                        "  Updated description  ",
                    Category =
                        HabitCategory.LearningAndSkills,
                    FrequencyType =
                        HabitFrequencyType.Weekly,
                    TargetCount = 4,
                    Difficulty =
                        HabitDifficulty.Elite
                });

        Assert.NotNull(response);

        Assert.Equal(originalId, habit.Id);
        Assert.Equal(originalUserId, habit.UserId);
        Assert.Equal(
            originalCreatedAtUtc,
            habit.CreatedAtUtc);
        Assert.Equal(
            originalIsActive,
            habit.IsActive);

        Assert.Equal(
            "Updated habit",
            habit.Name);

        Assert.Equal(
            "Updated description",
            habit.Description);

        Assert.Equal(
            HabitCategory.GeneralGrowth,
            habit.Category);

        Assert.Equal(
            HabitFrequencyType.Daily,
            habit.FrequencyType);

        Assert.Equal(
            1,
            habit.TargetCount);

        Assert.Equal(
            HabitDifficulty.Medium,
            habit.Difficulty);

        Assert.True(
            habit.UpdatedAtUtc
                > previousUpdatedAtUtc);

        var configurations =
            dbContext.HabitConfigurationVersions
                .OrderBy(configuration =>
                    configuration.VersionNumber)
                .ToList();

        Assert.Equal(
            2,
            configurations.Count);

        var currentConfiguration =
            configurations[0];

        Assert.Equal(
            1,
            currentConfiguration.VersionNumber);

        Assert.Equal(
            new DateOnly(2026, 7, 20),
            currentConfiguration
                .EffectiveToDateExclusive);

        Assert.Equal(
            HabitCategory.GeneralGrowth,
            currentConfiguration.Category);

        Assert.Equal(
            HabitFrequencyType.Daily,
            currentConfiguration.FrequencyType);

        Assert.Equal(
            1,
            currentConfiguration.TargetCount);

        Assert.Equal(
            HabitDifficulty.Medium,
            currentConfiguration.Difficulty);

        var pendingConfiguration =
            configurations[1];

        Assert.Equal(
            2,
            pendingConfiguration.VersionNumber);

        Assert.Equal(
            new DateOnly(2026, 7, 20),
            pendingConfiguration.EffectiveFromDate);

        Assert.Null(
            pendingConfiguration
                .EffectiveToDateExclusive);

        Assert.Equal(
            HabitCategory.LearningAndSkills,
            pendingConfiguration.Category);

        Assert.Equal(
            HabitFrequencyType.Weekly,
            pendingConfiguration.FrequencyType);

        Assert.Equal(
            4,
            pendingConfiguration.TargetCount);

        Assert.Equal(
            HabitDifficulty.Elite,
            pendingConfiguration.Difficulty);

        Assert.Equal(
            habit.Id,
            response.Id);

        Assert.Equal(
            habit.Name,
            response.Name);

        Assert.Equal(
            habit.Description,
            response.Description);

        Assert.Equal(
            habit.Category,
            response.Category);

        Assert.Equal(
            habit.FrequencyType,
            response.FrequencyType);

        Assert.Equal(
            habit.TargetCount,
            response.TargetCount);

        Assert.Equal(
            habit.Difficulty,
            response.Difficulty);

        Assert.Equal(
            habit.IsActive,
            response.IsActive);

        Assert.Equal(
            habit.CreatedAtUtc,
            response.CreatedAtUtc);

        Assert.Equal(
            habit.UpdatedAtUtc,
            response.UpdatedAtUtc);

        Assert.NotNull(
            response.PendingConfiguration);

        Assert.Equal(
            pendingConfiguration.EffectiveFromDate,
            response.PendingConfiguration
                .EffectiveFromDate);

        Assert.Equal(
            pendingConfiguration.Category,
            response.PendingConfiguration.Category);

        Assert.Equal(
            pendingConfiguration.FrequencyType,
            response.PendingConfiguration
                .FrequencyType);

        Assert.Equal(
            pendingConfiguration.TargetCount,
            response.PendingConfiguration
                .TargetCount);

        Assert.Equal(
            pendingConfiguration.Difficulty,
            response.PendingConfiguration
                .Difficulty);
    }


    [Fact]
    public async Task UpdateHabitAsync_WhenDescriptionIsBlank_StoresNull()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        var habit =
            CreateHabit(
                userId,
                "Existing habit",
                TestUtcNow.AddDays(-1),
                isActive: true);

        habit.Description = "Existing description";
        habit.Category =
    HabitCategory.GeneralGrowth;

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var habitService = CreateHabitService(dbContext);

        var request = new UpdateHabitRequest
        {
            Name = "Existing habit",
            Description = "   ",
            Category =
    HabitCategory.GeneralGrowth,
            FrequencyType =
                HabitFrequencyType.Daily,
            TargetCount = 1,
            Difficulty =
                HabitDifficulty.Medium
        };

        var response =
            await habitService.UpdateHabitAsync(
                userId,
                habit.Id,
                request);

        Assert.NotNull(response);
        Assert.Null(habit.Description);
        Assert.Null(response.Description);
    }

    [Fact]
    public async Task UpdateHabitAsync_WhenHabitBelongsToAnotherUser_ReturnsNullAndDoesNotModifyHabit()
    {
        await using var dbContext = CreateDbContext();

        var ownerUserId = Guid.CreateVersion7();
        var requestingUserId = Guid.CreateVersion7();
        var createdAtUtc = TestUtcNow.AddDays(-1);

        var habit =
            CreateHabit(
                ownerUserId,
                "Private habit",
                createdAtUtc,
                isActive: true);

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var habitService = CreateHabitService(dbContext);

        var request = new UpdateHabitRequest
        {
            Name = "Attempted update",
            Description = null,
            Category = null,
            FrequencyType =
                HabitFrequencyType.Weekly,
            TargetCount = 3,
            Difficulty =
                HabitDifficulty.Hard
        };

        var response =
            await habitService.UpdateHabitAsync(
                requestingUserId,
                habit.Id,
                request);

        Assert.Null(response);
        Assert.Equal("Private habit", habit.Name);
        Assert.Equal(
            HabitFrequencyType.Daily,
            habit.FrequencyType);
        Assert.Equal(1, habit.TargetCount);
        Assert.Equal(createdAtUtc, habit.UpdatedAtUtc);
    }

    [Fact]
    public async Task UpdateHabitAsync_WhenHabitDoesNotExist_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();

        var habitService = CreateHabitService(dbContext);

        var response =
            await habitService.UpdateHabitAsync(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                CreateValidUpdateRequest());

        Assert.Null(response);
        Assert.Empty(dbContext.Habits);
    }

    [Fact]
    public async Task UpdateHabitAsync_WhenNameIsWhitespace_ThrowsInvalidHabitNameExceptionAndDoesNotModifyHabit()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();
        var createdAtUtc = TestUtcNow.AddDays(-1);

        var habit =
            CreateHabit(
                userId,
                "Original name",
                createdAtUtc,
                isActive: true);

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var habitService = CreateHabitService(dbContext);

        var request = CreateValidUpdateRequest();
        request.Name = "   ";

        await Assert.ThrowsAsync<InvalidHabitNameException>(
            () =>
                habitService.UpdateHabitAsync(
                    userId,
                    habit.Id,
                    request));

        Assert.Equal("Original name", habit.Name);
        Assert.Equal(createdAtUtc, habit.UpdatedAtUtc);
    }

    [Theory]
    [InlineData(HabitFrequencyType.Daily, 2)]
    [InlineData(HabitFrequencyType.Weekly, 0)]
    [InlineData(HabitFrequencyType.Weekly, 8)]
    [InlineData((HabitFrequencyType)999, 1)]
    public async Task UpdateHabitAsync_WhenTargetConfigurationIsInvalid_ThrowsInvalidHabitTargetCountExceptionAndDoesNotModifyHabit(
        HabitFrequencyType frequencyType,
        int targetCount)
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();
        var createdAtUtc = TestUtcNow.AddDays(-1);

        var habit =
            CreateHabit(
                userId,
                "Original name",
                createdAtUtc,
                isActive: true);

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var habitService = CreateHabitService(dbContext);

        var request = CreateValidUpdateRequest();
        request.FrequencyType = frequencyType;
        request.TargetCount = targetCount;

        await Assert.ThrowsAsync<InvalidHabitTargetCountException>(
            () =>
                habitService.UpdateHabitAsync(
                    userId,
                    habit.Id,
                    request));

        Assert.Equal("Original name", habit.Name);
        Assert.Equal(
            HabitFrequencyType.Daily,
            habit.FrequencyType);
        Assert.Equal(1, habit.TargetCount);
        Assert.Equal(createdAtUtc, habit.UpdatedAtUtc);
    }


    [Fact]
    public async Task UpdateHabitAsync_WhenPendingConfigurationExists_UpdatesSamePendingVersion()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        var habit =
            CreateHabit(
                userId,
                "Existing habit",
                new DateTime(
                    2026,
                    7,
                    18,
                    2,
                    30,
                    0,
                    DateTimeKind.Utc),
                isActive: true);

        dbContext.Habits.Add(habit);

        await dbContext.SaveChangesAsync();

        var habitService = CreateHabitService(dbContext);

        await habitService.UpdateHabitAsync(
            userId,
            habit.Id,
            new UpdateHabitRequest
            {
                Name = habit.Name,
                Description = null,
                Category =
                    HabitCategory.LearningAndSkills,
                FrequencyType =
                    HabitFrequencyType.Weekly,
                TargetCount = 3,
                Difficulty =
                    HabitDifficulty.Hard
            });

        var response =
            await habitService.UpdateHabitAsync(
                userId,
                habit.Id,
                new UpdateHabitRequest
                {
                    Name = habit.Name,
                    Description = null,
                    Category =
                        HabitCategory.FitnessAndMovement,
                    FrequencyType =
                        HabitFrequencyType.Weekly,
                    TargetCount = 5,
                    Difficulty =
                        HabitDifficulty.Elite
                });

        Assert.Equal(
            2,
            dbContext.HabitConfigurationVersions.Count());

        var currentConfiguration =
            dbContext.HabitConfigurationVersions
                .Single(configuration =>
                    configuration.VersionNumber == 1);

        Assert.Equal(
            new DateOnly(2026, 7, 20),
            currentConfiguration
                .EffectiveToDateExclusive);

        var pendingConfiguration =
            dbContext.HabitConfigurationVersions
                .Single(configuration =>
                    configuration.VersionNumber == 2);

        Assert.Equal(
            new DateOnly(2026, 7, 20),
            pendingConfiguration.EffectiveFromDate);

        Assert.Equal(
            HabitCategory.FitnessAndMovement,
            pendingConfiguration.Category);

        Assert.Equal(
            HabitFrequencyType.Weekly,
            pendingConfiguration.FrequencyType);

        Assert.Equal(
            5,
            pendingConfiguration.TargetCount);

        Assert.Equal(
            HabitDifficulty.Elite,
            pendingConfiguration.Difficulty);

        Assert.NotNull(
            response!.PendingConfiguration);

        Assert.Equal(
            HabitCategory.FitnessAndMovement,
            response.PendingConfiguration.Category);

        Assert.Equal(
            5,
            response.PendingConfiguration.TargetCount);
    }

    [Fact]
    public async Task UpdateHabitAsync_WhenRequestedRulesMatchCurrent_RemovesPendingVersion()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        var habit =
            CreateHabit(
                userId,
                "Existing habit",
                new DateTime(
                    2026,
                    7,
                    18,
                    2,
                    30,
                    0,
                    DateTimeKind.Utc),
                isActive: true);

        dbContext.Habits.Add(habit);

        await dbContext.SaveChangesAsync();

        var habitService = CreateHabitService(dbContext);

        await habitService.UpdateHabitAsync(
            userId,
            habit.Id,
            CreateValidUpdateRequest());

        var response =
            await habitService.UpdateHabitAsync(
                userId,
                habit.Id,
                new UpdateHabitRequest
                {
                    Name = habit.Name,
                    Description = null,
                    Category =
                        HabitCategory.GeneralGrowth,
                    FrequencyType =
                        HabitFrequencyType.Daily,
                    TargetCount = 1,
                    Difficulty =
                        HabitDifficulty.Medium
                });

        var currentConfiguration =
            Assert.Single(
                dbContext.HabitConfigurationVersions);

        Assert.Equal(
            1,
            currentConfiguration.VersionNumber);

        Assert.Null(
            currentConfiguration
                .EffectiveToDateExclusive);

        Assert.Null(
            response!.PendingConfiguration);
    }

    [Fact]
    public async Task DeactivateHabitAsync_WhenOwnedHabitIsActive_DeactivatesHabitAndUpdatesTimestamp()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        var createdAtUtc = TestUtcNow.AddDays(-2);
        var previousUpdatedAtUtc =
            TestUtcNow.AddDays(-1);

        var habit =
            CreateHabit(
                userId,
                "Active habit",
                createdAtUtc,
                isActive: true);

        habit.UpdatedAtUtc =
            previousUpdatedAtUtc;

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var originalId = habit.Id;
        var originalUserId = habit.UserId;
        var originalCreatedAtUtc = habit.CreatedAtUtc;
        var originalName = habit.Name;

        var habitService =
            CreateHabitService(dbContext);

        var response =
            await habitService.DeactivateHabitAsync(
                userId,
                habit.Id);

        Assert.NotNull(response);

        var savedHabit =
            Assert.Single(dbContext.Habits);

        Assert.Equal(originalId, savedHabit.Id);
        Assert.Equal(
            originalUserId,
            savedHabit.UserId);
        Assert.Equal(
            originalCreatedAtUtc,
            savedHabit.CreatedAtUtc);
        Assert.Equal(
            originalName,
            savedHabit.Name);

        Assert.False(savedHabit.IsActive);

        Assert.True(
            savedHabit.UpdatedAtUtc
                > previousUpdatedAtUtc);

        Assert.Equal(
            savedHabit.Id,
            response.Id);

        Assert.False(response.IsActive);

        Assert.Equal(
            savedHabit.UpdatedAtUtc,
            response.UpdatedAtUtc);
    }

    [Fact]
    public async Task DeactivateHabitAsync_WhenOwnedHabitIsAlreadyInactive_ReturnsUnchangedHabit()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        var createdAtUtc =
            TestUtcNow.AddDays(-2);
        var previousUpdatedAtUtc =
            TestUtcNow.AddDays(-1);

        var habit =
            CreateHabit(
                userId,
                "Inactive habit",
                createdAtUtc,
                isActive: false);

        habit.UpdatedAtUtc =
            previousUpdatedAtUtc;

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var habitService =
            CreateHabitService(dbContext);

        var response =
            await habitService.DeactivateHabitAsync(
                userId,
                habit.Id);

        Assert.NotNull(response);

        var savedHabit =
            Assert.Single(dbContext.Habits);

        Assert.False(savedHabit.IsActive);

        Assert.Equal(
            previousUpdatedAtUtc,
            savedHabit.UpdatedAtUtc);

        Assert.False(response.IsActive);

        Assert.Equal(
            previousUpdatedAtUtc,
            response.UpdatedAtUtc);
    }

    [Fact]
    public async Task DeactivateHabitAsync_WhenHabitBelongsToAnotherUser_ReturnsNullAndDoesNotModifyHabit()
    {
        await using var dbContext = CreateDbContext();

        var ownerUserId =
            Guid.CreateVersion7();

        var requestingUserId =
            Guid.CreateVersion7();

        var createdAtUtc =
            TestUtcNow.AddDays(-1);

        var habit =
            CreateHabit(
                ownerUserId,
                "Private habit",
                createdAtUtc,
                isActive: true);

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var habitService =
            CreateHabitService(dbContext);

        var response =
            await habitService.DeactivateHabitAsync(
                requestingUserId,
                habit.Id);

        Assert.Null(response);

        Assert.True(habit.IsActive);

        Assert.Equal(
            createdAtUtc,
            habit.UpdatedAtUtc);
    }

    [Fact]
    public async Task DeactivateHabitAsync_WhenHabitDoesNotExist_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();

        var habitService =
            CreateHabitService(dbContext);

        var response =
            await habitService.DeactivateHabitAsync(
                Guid.CreateVersion7(),
                Guid.CreateVersion7());

        Assert.Null(response);
        Assert.Empty(dbContext.Habits);
    }

    [Fact]
    public async Task DeactivateHabitAsync_WhenHabitIsCompletedToday_ReturnsCompletedStatus()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        var habit =
            CreateHabit(
                userId,
                "Completed habit",
                TestUtcNow.AddDays(-1),
                isActive: true);

        dbContext.Habits.Add(habit);

        dbContext.HabitCompletions.Add(
            new HabitCompletion
            {
                UserId = userId,
                HabitId = habit.Id,
                CompletedDate =
                    new DateOnly(2026, 7, 19),
                CompletedAtUtc =
                    new DateTime(
                        2026,
                        7,
                        20,
                        1,
                        30,
                        0,
                        DateTimeKind.Utc)
            });

        await dbContext.SaveChangesAsync();

        var habitService =
            CreateHabitService(dbContext);

        var response =
            await habitService.DeactivateHabitAsync(
                userId,
                habit.Id);

        Assert.NotNull(response);
        Assert.False(response.IsActive);
        Assert.True(response.IsCompletedToday);

        Assert.Single(
            dbContext.HabitCompletions,
            completion =>
                completion.HabitId == habit.Id);
    }

    [Fact]
    public async Task ActivateHabitAsync_WhenOwnedHabitIsInactive_ActivatesHabitAndUpdatesTimestamp()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        var previousUpdatedAtUtc =
            TestUtcNow.AddDays(-1);

        var habit =
            CreateHabit(
                userId,
                "Inactive habit",
                TestUtcNow.AddDays(-2),
                isActive: false);

        habit.UpdatedAtUtc =
            previousUpdatedAtUtc;

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var habitService =
            CreateHabitService(dbContext);

        var response =
            await habitService.ActivateHabitAsync(
                userId,
                habit.Id);

        Assert.NotNull(response);
        Assert.True(habit.IsActive);
        Assert.True(response.IsActive);

        Assert.True(
            habit.UpdatedAtUtc
                > previousUpdatedAtUtc);

        Assert.Equal(
            habit.UpdatedAtUtc,
            response.UpdatedAtUtc);
    }

    [Fact]
    public async Task ActivateHabitAsync_WhenOwnedHabitIsAlreadyActive_ReturnsUnchangedHabit()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        var previousUpdatedAtUtc =
            TestUtcNow.AddDays(-1);

        var habit =
            CreateHabit(
                userId,
                "Active habit",
                TestUtcNow.AddDays(-2),
                isActive: true);

        habit.UpdatedAtUtc =
            previousUpdatedAtUtc;

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var habitService =
            CreateHabitService(dbContext);

        var response =
            await habitService.ActivateHabitAsync(
                userId,
                habit.Id);

        Assert.NotNull(response);
        Assert.True(response.IsActive);

        Assert.Equal(
            previousUpdatedAtUtc,
            response.UpdatedAtUtc);
    }

    [Fact]
    public async Task ActivateHabitAsync_WhenHabitBelongsToAnotherUser_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();

        var ownerUserId =
            Guid.CreateVersion7();

        var requestingUserId =
            Guid.CreateVersion7();

        var habit =
            CreateHabit(
                ownerUserId,
                "Private inactive habit",
                TestUtcNow.AddDays(-1),
                isActive: false);

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var habitService =
            CreateHabitService(dbContext);

        var response =
            await habitService.ActivateHabitAsync(
                requestingUserId,
                habit.Id);

        Assert.Null(response);
        Assert.False(habit.IsActive);
    }

    private static HabitService CreateHabitService(
        AppDbContext dbContext)
    {
        var utcNow =
            new DateTimeOffset(
                2026,
                7,
                20,
                2,
                30,
                0,
                TimeSpan.Zero);

        return new HabitService(
    dbContext,
    new FixedTimeProvider(utcNow),
    new XpService());
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

    private static void AddUserWithSettings(
    AppDbContext dbContext,
    Guid userId)
    {
        var timestampUtc =
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
            CreatedAtUtc =
                timestampUtc
        };

        var settings = new UserSettings
        {
            UserId = userId,
            DisplayName =
                $"User {uniqueSuffix[..8]}",
            TimeZone =
                "America/Toronto",
            CreatedAtUtc =
                timestampUtc,
            UpdatedAtUtc =
                timestampUtc,
            User = user
        };

        user.UserSettings = settings;

        dbContext.Users.Add(user);
    }

    private static Habit CreateHabit(
        Guid userId,
        string name,
        DateTime createdAtUtc,
        bool isActive)
    {
        var habit =
            new Habit
            {
                UserId = userId,
                Name = name,
                Category =
                    HabitCategory.GeneralGrowth,
                FrequencyType =
                    HabitFrequencyType.Daily,
                TargetCount = 1,
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
                FrequencyType =
                    habit.FrequencyType,
                TargetCount = habit.TargetCount,
                Difficulty = habit.Difficulty,
                EffectiveFromDate =
                    DateOnly.FromDateTime(
                        createdAtUtc),
                CreatedAtUtc = createdAtUtc
            });

        return habit;
    }

    private static CreateHabitRequest CreateValidRequest()
    {
        return new CreateHabitRequest
        {
            Name = "Read C# textbook",
            Description = "Read one chapter.",
            Category =
    HabitCategory.LearningAndSkills,
            FrequencyType = HabitFrequencyType.Daily,
            TargetCount = 1,
            Difficulty = HabitDifficulty.Medium
        };
    }

    private static UpdateHabitRequest CreateValidUpdateRequest()
    {
        return new UpdateHabitRequest
        {
            Name = "Updated habit",
            Description = "Updated description",
            Category =
    HabitCategory.LearningAndSkills,
            FrequencyType =
                HabitFrequencyType.Weekly,
            TargetCount = 3,
            Difficulty =
                HabitDifficulty.Hard
        };
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
