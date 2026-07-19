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

        var habitService = new HabitService(dbContext);
        var userId = Guid.CreateVersion7();
        var beforeCreationUtc = DateTime.UtcNow;

        var request = new CreateHabitRequest
        {
            Name = "  Go to gym  ",
            Description = "  Complete a planned workout.  ",
            Category = "  Fitness  ",
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
        Assert.Equal("Fitness", savedHabit.Category);
        Assert.Equal(
            HabitFrequencyType.Weekly,
            savedHabit.FrequencyType);
        Assert.Equal(3, savedHabit.TargetCount);
        Assert.Equal(
            HabitDifficulty.Elite,
            savedHabit.Difficulty);
        Assert.True(savedHabit.IsActive);

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
        Assert.Equal(savedHabit.IsActive, response.IsActive);
        Assert.Equal(
            savedHabit.CreatedAtUtc,
            response.CreatedAtUtc);
        Assert.Equal(
            savedHabit.UpdatedAtUtc,
            response.UpdatedAtUtc);
    }

    [Fact]
    public async Task CreateHabitAsync_WhenOptionalTextIsBlank_StoresNull()
    {
        await using var dbContext = CreateDbContext();

        var habitService = new HabitService(dbContext);

        var request = CreateValidRequest();

        request.Description = "   ";
        request.Category = string.Empty;

        var response =
            await habitService.CreateHabitAsync(
                Guid.CreateVersion7(),
                request);

        var savedHabit =
            Assert.Single(dbContext.Habits);

        Assert.Null(savedHabit.Description);
        Assert.Null(savedHabit.Category);
        Assert.Null(response.Description);
        Assert.Null(response.Category);
    }

    [Fact]
    public async Task CreateHabitAsync_WhenNameIsWhitespace_ThrowsInvalidHabitNameException()
    {
        await using var dbContext = CreateDbContext();

        var habitService = new HabitService(dbContext);

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

        var habitService = new HabitService(dbContext);

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

        var habitService = new HabitService(dbContext);

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

        var habitService = new HabitService(dbContext);

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

        var habit =
            CreateHabit(
                userId,
                "Read C# textbook",
                DateTime.UtcNow,
                isActive: false);

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var habitService = new HabitService(dbContext);

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

        var habitService = new HabitService(dbContext);

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

        var habitService = new HabitService(dbContext);

        var response =
            await habitService.GetUserHabitAsync(
                Guid.CreateVersion7(),
                Guid.CreateVersion7());

        Assert.Null(response);
    }

    [Fact]
    public async Task UpdateHabitAsync_WhenHabitBelongsToUser_UpdatesEditableFieldsAndPreservesProtectedFields()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();
        var createdAtUtc = DateTime.UtcNow.AddDays(-2);
        var previousUpdatedAtUtc = createdAtUtc.AddDays(1);

        var habit =
            CreateHabit(
                userId,
                "Old name",
                createdAtUtc,
                isActive: false);

        habit.Description = "Old description";
        habit.Category = "Old category";
        habit.UpdatedAtUtc = previousUpdatedAtUtc;

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var originalId = habit.Id;
        var originalUserId = habit.UserId;
        var originalCreatedAtUtc = habit.CreatedAtUtc;
        var originalIsActive = habit.IsActive;

        var habitService = new HabitService(dbContext);

        var request = new UpdateHabitRequest
        {
            Name = "  Updated habit  ",
            Description = "  Updated description  ",
            Category = "  Updated category  ",
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
            "Updated category",
            savedHabit.Category);
        Assert.Equal(
            HabitFrequencyType.Weekly,
            savedHabit.FrequencyType);
        Assert.Equal(4, savedHabit.TargetCount);
        Assert.Equal(
            HabitDifficulty.Elite,
            savedHabit.Difficulty);

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
    public async Task UpdateHabitAsync_WhenOptionalTextIsBlank_StoresNull()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        var habit =
            CreateHabit(
                userId,
                "Existing habit",
                DateTime.UtcNow.AddDays(-1),
                isActive: true);

        habit.Description = "Existing description";
        habit.Category = "Existing category";

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var habitService = new HabitService(dbContext);

        var request = new UpdateHabitRequest
        {
            Name = "Existing habit",
            Description = "   ",
            Category = string.Empty,
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
        Assert.Null(habit.Category);
        Assert.Null(response.Description);
        Assert.Null(response.Category);
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

        var habitService = new HabitService(dbContext);

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

        var habitService = new HabitService(dbContext);

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

        var habitService = new HabitService(dbContext);

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

        var habitService = new HabitService(dbContext);

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

    private static AppDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(
                    Guid.NewGuid().ToString())
                .Options;

        return new AppDbContext(options);
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
            Category = "Learning",
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
            Category = "Updated category",
            FrequencyType =
                HabitFrequencyType.Weekly,
            TargetCount = 3,
            Difficulty =
                HabitDifficulty.Hard
        };
    }
}