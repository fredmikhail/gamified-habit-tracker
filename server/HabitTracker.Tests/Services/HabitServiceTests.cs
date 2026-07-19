using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.DTOs;
using HabitTracker.Api.Exceptions;
using HabitTracker.Api.Services;
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

    private static AppDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(
                    Guid.NewGuid().ToString())
                .Options;

        return new AppDbContext(options);
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
}