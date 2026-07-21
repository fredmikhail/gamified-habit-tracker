using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.DTOs;
using HabitTracker.Api.Exceptions;
using HabitTracker.Api.Services;
using HabitTracker.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Tests.Services;

public sealed class HabitServiceTests
{
    [Fact]
    public async Task CreateHabitAsync_WhenRequestIsValid_CreatesAndReturnsHabit()
    {
        await using var dbContext = CreateDbContext();

        var habitService = CreateHabitService(dbContext);
        var userId = Guid.CreateVersion7();
        var beforeCreationUtc = DateTime.UtcNow;

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

        Assert.InRange(
            savedHabit.CreatedAtUtc,
            beforeCreationUtc,
            DateTime.UtcNow);

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

        var habitService = CreateHabitService(dbContext);

        var request = CreateValidRequest();

        request.Description = "   ";

        var response =
            await habitService.CreateHabitAsync(
                Guid.CreateVersion7(),
                request);

        var savedHabit =
            Assert.Single(dbContext.Habits);

        Assert.Null(savedHabit.Description);
        Assert.Null(response.Description);
    }

    [Fact]
    public async Task CreateHabitAsync_WhenNameIsWhitespace_ThrowsInvalidHabitNameException()
    {
        await using var dbContext = CreateDbContext();

        var habitService = CreateHabitService(dbContext);

        var request = CreateValidRequest();

        request.Name = "   ";

        await Assert.ThrowsAsync<InvalidHabitNameException>(
            () =>
                habitService.CreateHabitAsync(
                    Guid.CreateVersion7(),
                    request));

        Assert.Empty(dbContext.Habits);
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

        var habitService = CreateHabitService(dbContext);

        var request = CreateValidRequest();

        request.FrequencyType = frequencyType;
        request.TargetCount = targetCount;

        await Assert.ThrowsAsync<InvalidHabitTargetCountException>(
            () =>
                habitService.CreateHabitAsync(
                    Guid.CreateVersion7(),
                    request));

        Assert.Empty(dbContext.Habits);
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
        var baseTimestampUtc = DateTime.UtcNow.AddDays(-1);

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
        var baseTimestampUtc = DateTime.UtcNow.AddDays(-1);

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
                DateTime.UtcNow,
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
                DateTime.UtcNow,
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
                DateTime.UtcNow.AddHours(-2),
                isActive: true);

        var incompleteHabit =
            CreateHabit(
                userId,
                "Incomplete habit",
                DateTime.UtcNow.AddHours(-1),
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
                DateTime.UtcNow.AddDays(-1),
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
                DateTime.UtcNow.AddDays(-1),
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
    public async Task UpdateHabitAsync_WhenHabitBelongsToUser_UpdatesEditableFieldsAndPreservesProtectedFields()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        var createdAtUtc = DateTime.UtcNow.AddDays(-2);
        var previousUpdatedAtUtc = createdAtUtc.AddDays(1);

        var habit =
            CreateHabit(
                userId,
                "Old name",
                createdAtUtc,
                isActive: false);

        habit.Description = "Old description";
        habit.Category =
    HabitCategory.GeneralGrowth;
        habit.UpdatedAtUtc = previousUpdatedAtUtc;

        habit.HabitAttributeRewards.Add(
    new HabitAttributeReward
    {
        HabitId = habit.Id,
        AttributeType =
            AttributeType.Discipline,
        XpAmount = 14
    });

        habit.HabitAttributeRewards.Add(
            new HabitAttributeReward
            {
                HabitId = habit.Id,
                AttributeType =
                    AttributeType.Mind,
                XpAmount = 6
            });

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var originalId = habit.Id;
        var originalUserId = habit.UserId;
        var originalCreatedAtUtc = habit.CreatedAtUtc;
        var originalIsActive = habit.IsActive;

        var habitService = CreateHabitService(dbContext);

        var request = new UpdateHabitRequest
        {
            Name = "  Updated habit  ",
            Description = "  Updated description  ",
            Category =
    HabitCategory.LearningAndSkills,
            FrequencyType =
                HabitFrequencyType.Weekly,
            TargetCount = 4,
            Difficulty =
                HabitDifficulty.Elite
        };

        var response =
            await habitService.UpdateHabitAsync(
                userId,
                habit.Id,
                request);

        Assert.NotNull(response);

        var savedHabit =
            Assert.Single(dbContext.Habits);

        Assert.Equal(originalId, savedHabit.Id);
        Assert.Equal(originalUserId, savedHabit.UserId);
        Assert.Equal(
            originalCreatedAtUtc,
            savedHabit.CreatedAtUtc);
        Assert.Equal(
            originalIsActive,
            savedHabit.IsActive);

        Assert.Equal("Updated habit", savedHabit.Name);
        Assert.Equal(
            "Updated description",
            savedHabit.Description);
        Assert.Equal(
    HabitCategory.LearningAndSkills,
    savedHabit.Category);
        Assert.Equal(
            HabitFrequencyType.Weekly,
            savedHabit.FrequencyType);
        Assert.Equal(4, savedHabit.TargetCount);
        Assert.Equal(
            HabitDifficulty.Elite,
            savedHabit.Difficulty);

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
            savedRewards[AttributeType.Mind]);

        Assert.Equal(
            15,
            savedRewards[AttributeType.Focus]);

        Assert.DoesNotContain(
            AttributeType.Discipline,
            savedRewards.Keys);

        Assert.True(
            savedHabit.UpdatedAtUtc
                > previousUpdatedAtUtc);

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
        Assert.Equal(savedHabit.IsActive, response.IsActive);
        Assert.Equal(
            savedHabit.CreatedAtUtc,
            response.CreatedAtUtc);
        Assert.Equal(
            savedHabit.UpdatedAtUtc,
            response.UpdatedAtUtc);
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
                DateTime.UtcNow.AddDays(-1),
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
        var createdAtUtc = DateTime.UtcNow.AddDays(-1);

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
        var createdAtUtc = DateTime.UtcNow.AddDays(-1);

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
        var createdAtUtc = DateTime.UtcNow.AddDays(-1);

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
    public async Task DeactivateHabitAsync_WhenOwnedHabitIsActive_DeactivatesHabitAndUpdatesTimestamp()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId);

        var createdAtUtc = DateTime.UtcNow.AddDays(-2);
        var previousUpdatedAtUtc =
            DateTime.UtcNow.AddDays(-1);

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
            DateTime.UtcNow.AddDays(-2);
        var previousUpdatedAtUtc =
            DateTime.UtcNow.AddDays(-1);

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
            DateTime.UtcNow.AddDays(-1);

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
                DateTime.UtcNow.AddDays(-1),
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
        return new Habit
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
