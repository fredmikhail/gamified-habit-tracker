using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.DTOs;
using HabitTracker.Api.Exceptions;
using HabitTracker.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Tests.Services;

public sealed class CompletionServiceTests
{
    [Fact]
    public async Task CompleteHabitAsync_WhenRequestIsValid_CreatesCompletionUsingUserLocalDate()
    {
        await using var dbContext = CreateDbContext();

        var utcNow = new DateTimeOffset(
            2026,
            7,
            20,
            2,
            30,
            0,
            TimeSpan.Zero);

        var userId = Guid.CreateVersion7();
        var habitId = Guid.CreateVersion7();

        var user = new User
        {
            Id = userId,
            Email = "fred@example.com",
            NormalizedEmail = "FRED@EXAMPLE.COM",
            Username = "fred",
            NormalizedUsername = "FRED",
            PasswordHash = "test-password-hash",
            CreatedAtUtc = utcNow.UtcDateTime.AddDays(-1)
        };

        var userSettings = new UserSettings
        {
            UserId = userId,
            DisplayName = "Fred",
            TimeZone = "America/Toronto",
            CreatedAtUtc = utcNow.UtcDateTime.AddDays(-1),
            UpdatedAtUtc = utcNow.UtcDateTime.AddDays(-1)
        };

        var habit = new Habit
        {
            Id = habitId,
            UserId = userId,
            Name = "Go to gym",
            Category = "Fitness",
            FrequencyType = HabitFrequencyType.Daily,
            TargetCount = 1,
            Difficulty = HabitDifficulty.Medium,
            IsActive = true,
            CreatedAtUtc = utcNow.UtcDateTime.AddDays(-1),
            UpdatedAtUtc = utcNow.UtcDateTime.AddDays(-1)
        };

        dbContext.Users.Add(user);
        dbContext.UserSettings.Add(userSettings);
        dbContext.Habits.Add(habit);

        await dbContext.SaveChangesAsync();

        var timeProvider =
            new FixedTimeProvider(utcNow);

        var completionService =
            new CompletionService(
                dbContext,
                timeProvider);

        var request = new CompleteHabitRequest
        {
            Notes = "  Strong workout today.  "
        };

        var response =
            await completionService.CompleteHabitAsync(
                userId,
                habitId,
                request);

        Assert.NotNull(response);

        var savedCompletion =
            Assert.Single(dbContext.HabitCompletions);

        Assert.NotEqual(
            Guid.Empty,
            savedCompletion.Id);

        Assert.Equal(
            userId,
            savedCompletion.UserId);

        Assert.Equal(
            habitId,
            savedCompletion.HabitId);

        Assert.Equal(
            new DateOnly(2026, 7, 19),
            savedCompletion.CompletedDate);

        Assert.Equal(
            utcNow.UtcDateTime,
            savedCompletion.CompletedAtUtc);

        Assert.Equal(
            "Strong workout today.",
            savedCompletion.Notes);

        Assert.Equal(
            savedCompletion.Id,
            response.Completion.Id);

        Assert.Equal(
            savedCompletion.HabitId,
            response.Completion.HabitId);

        Assert.Equal(
            savedCompletion.CompletedDate,
            response.Completion.CompletedDate);

        Assert.Equal(
            savedCompletion.CompletedAtUtc,
            response.Completion.CompletedAtUtc);

        Assert.Equal(
            savedCompletion.Notes,
            response.Completion.Notes);
    }

    [Fact]
    public async Task CompleteHabitAsync_WhenNotesAreBlank_StoresNull()
    {
        await using var dbContext = CreateDbContext();

        var utcNow = new DateTimeOffset(
            2026,
            7,
            20,
            2,
            30,
            0,
            TimeSpan.Zero);

        var userId = Guid.CreateVersion7();
        var habitId = Guid.CreateVersion7();

        dbContext.Users.Add(
            new User
            {
                Id = userId,
                Email = "fred@example.com",
                NormalizedEmail = "FRED@EXAMPLE.COM",
                Username = "fred",
                NormalizedUsername = "FRED",
                PasswordHash = "test-password-hash",
                CreatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1)
            });

        dbContext.UserSettings.Add(
            new UserSettings
            {
                UserId = userId,
                DisplayName = "Fred",
                TimeZone = "America/Toronto",
                CreatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1),
                UpdatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1)
            });

        dbContext.Habits.Add(
            new Habit
            {
                Id = habitId,
                UserId = userId,
                Name = "Go to gym",
                FrequencyType =
                    HabitFrequencyType.Daily,
                TargetCount = 1,
                Difficulty =
                    HabitDifficulty.Medium,
                IsActive = true,
                CreatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1),
                UpdatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1)
            });

        await dbContext.SaveChangesAsync();

        var completionService =
            new CompletionService(
                dbContext,
                new FixedTimeProvider(utcNow));

        var response =
            await completionService.CompleteHabitAsync(
                userId,
                habitId,
                new CompleteHabitRequest
                {
                    Notes = "     "
                });

        var savedCompletion =
            Assert.Single(dbContext.HabitCompletions);

        Assert.Null(savedCompletion.Notes);
        Assert.Null(response!.Completion.Notes);
    }

    [Fact]
    public async Task CompleteHabitAsync_WhenHabitDoesNotExist_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();

        var utcNow = new DateTimeOffset(
            2026,
            7,
            20,
            2,
            30,
            0,
            TimeSpan.Zero);

        var userId = Guid.CreateVersion7();

        dbContext.Users.Add(
            new User
            {
                Id = userId,
                Email = "fred@example.com",
                NormalizedEmail = "FRED@EXAMPLE.COM",
                Username = "fred",
                NormalizedUsername = "FRED",
                PasswordHash = "test-password-hash",
                CreatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1)
            });

        dbContext.UserSettings.Add(
            new UserSettings
            {
                UserId = userId,
                DisplayName = "Fred",
                TimeZone = "America/Toronto",
                CreatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1),
                UpdatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1)
            });

        await dbContext.SaveChangesAsync();

        var completionService =
            new CompletionService(
                dbContext,
                new FixedTimeProvider(utcNow));

        var response =
            await completionService.CompleteHabitAsync(
                userId,
                Guid.CreateVersion7(),
                new CompleteHabitRequest());

        Assert.Null(response);
        Assert.Empty(dbContext.HabitCompletions);
    }

    [Fact]
    public async Task CompleteHabitAsync_WhenHabitBelongsToAnotherUser_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();

        var utcNow = new DateTimeOffset(
            2026,
            7,
            20,
            2,
            30,
            0,
            TimeSpan.Zero);

        var ownerUserId = Guid.CreateVersion7();
        var requestingUserId = Guid.CreateVersion7();
        var habitId = Guid.CreateVersion7();

        dbContext.Habits.Add(
            new Habit
            {
                Id = habitId,
                UserId = ownerUserId,
                Name = "Private habit",
                FrequencyType =
                    HabitFrequencyType.Daily,
                TargetCount = 1,
                Difficulty =
                    HabitDifficulty.Medium,
                IsActive = true,
                CreatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1),
                UpdatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1)
            });

        await dbContext.SaveChangesAsync();

        var completionService =
            new CompletionService(
                dbContext,
                new FixedTimeProvider(utcNow));

        var response =
            await completionService.CompleteHabitAsync(
                requestingUserId,
                habitId,
                new CompleteHabitRequest());

        Assert.Null(response);
        Assert.Empty(dbContext.HabitCompletions);
    }

    [Fact]
    public async Task CompleteHabitAsync_WhenHabitIsInactive_ThrowsInactiveHabitException()
    {
        await using var dbContext = CreateDbContext();

        var utcNow = new DateTimeOffset(
            2026,
            7,
            20,
            2,
            30,
            0,
            TimeSpan.Zero);

        var userId = Guid.CreateVersion7();
        var habitId = Guid.CreateVersion7();

        dbContext.Users.Add(
            new User
            {
                Id = userId,
                Email = "fred@example.com",
                NormalizedEmail = "FRED@EXAMPLE.COM",
                Username = "fred",
                NormalizedUsername = "FRED",
                PasswordHash = "test-password-hash",
                CreatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1)
            });

        dbContext.UserSettings.Add(
            new UserSettings
            {
                UserId = userId,
                DisplayName = "Fred",
                TimeZone = "America/Toronto",
                CreatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1),
                UpdatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1)
            });

        dbContext.Habits.Add(
            new Habit
            {
                Id = habitId,
                UserId = userId,
                Name = "Archived gym habit",
                FrequencyType =
                    HabitFrequencyType.Daily,
                TargetCount = 1,
                Difficulty =
                    HabitDifficulty.Medium,
                IsActive = false,
                CreatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1),
                UpdatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1)
            });

        await dbContext.SaveChangesAsync();

        var completionService =
            new CompletionService(
                dbContext,
                new FixedTimeProvider(utcNow));

        await Assert.ThrowsAsync<InactiveHabitException>(
            () =>
                completionService.CompleteHabitAsync(
                    userId,
                    habitId,
                    new CompleteHabitRequest()));

        Assert.Empty(dbContext.HabitCompletions);
    }

    [Fact]
    public async Task CompleteHabitAsync_WhenHabitIsAlreadyCompletedToday_ThrowsHabitAlreadyCompletedException()
    {
        await using var dbContext = CreateDbContext();

        var utcNow = new DateTimeOffset(
            2026,
            7,
            20,
            2,
            30,
            0,
            TimeSpan.Zero);

        var userId = Guid.CreateVersion7();
        var habitId = Guid.CreateVersion7();

        dbContext.Users.Add(
            new User
            {
                Id = userId,
                Email = "fred@example.com",
                NormalizedEmail = "FRED@EXAMPLE.COM",
                Username = "fred",
                NormalizedUsername = "FRED",
                PasswordHash = "test-password-hash",
                CreatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1)
            });

        dbContext.UserSettings.Add(
            new UserSettings
            {
                UserId = userId,
                DisplayName = "Fred",
                TimeZone = "America/Toronto",
                CreatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1),
                UpdatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1)
            });

        dbContext.Habits.Add(
            new Habit
            {
                Id = habitId,
                UserId = userId,
                Name = "Go to gym",
                FrequencyType =
                    HabitFrequencyType.Daily,
                TargetCount = 1,
                Difficulty =
                    HabitDifficulty.Medium,
                IsActive = true,
                CreatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1),
                UpdatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1)
            });

        dbContext.HabitCompletions.Add(
            new HabitCompletion
            {
                UserId = userId,
                HabitId = habitId,
                CompletedDate =
                    new DateOnly(2026, 7, 19),
                CompletedAtUtc =
                    utcNow.UtcDateTime.AddHours(-1)
            });

        await dbContext.SaveChangesAsync();

        var completionService =
            new CompletionService(
                dbContext,
                new FixedTimeProvider(utcNow));

        await Assert.ThrowsAsync<HabitAlreadyCompletedException>(
            () =>
                completionService.CompleteHabitAsync(
                    userId,
                    habitId,
                    new CompleteHabitRequest()));

        Assert.Single(dbContext.HabitCompletions);
    }

    [Fact]
    public async Task CompleteHabitAsync_WhenHabitWasCompletedYesterday_CreatesTodaysCompletion()
    {
        await using var dbContext = CreateDbContext();

        var utcNow = new DateTimeOffset(
            2026,
            7,
            20,
            2,
            30,
            0,
            TimeSpan.Zero);

        var userId = Guid.CreateVersion7();
        var habitId = Guid.CreateVersion7();

        dbContext.Users.Add(
            new User
            {
                Id = userId,
                Email = "fred@example.com",
                NormalizedEmail = "FRED@EXAMPLE.COM",
                Username = "fred",
                NormalizedUsername = "FRED",
                PasswordHash = "test-password-hash",
                CreatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-2)
            });

        dbContext.UserSettings.Add(
            new UserSettings
            {
                UserId = userId,
                DisplayName = "Fred",
                TimeZone = "America/Toronto",
                CreatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-2),
                UpdatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-2)
            });

        dbContext.Habits.Add(
            new Habit
            {
                Id = habitId,
                UserId = userId,
                Name = "Go to gym",
                FrequencyType =
                    HabitFrequencyType.Daily,
                TargetCount = 1,
                Difficulty =
                    HabitDifficulty.Medium,
                IsActive = true,
                CreatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-2),
                UpdatedAtUtc =
                    utcNow.UtcDateTime.AddDays(-2)
            });

        dbContext.HabitCompletions.Add(
            new HabitCompletion
            {
                UserId = userId,
                HabitId = habitId,
                CompletedDate =
                    new DateOnly(2026, 7, 18),
                CompletedAtUtc =
                    utcNow.UtcDateTime.AddDays(-1)
            });

        await dbContext.SaveChangesAsync();

        var completionService =
            new CompletionService(
                dbContext,
                new FixedTimeProvider(utcNow));

        var response =
            await completionService.CompleteHabitAsync(
                userId,
                habitId,
                new CompleteHabitRequest());

        Assert.NotNull(response);

        Assert.Equal(
            new DateOnly(2026, 7, 19),
            response.Completion.CompletedDate);

        Assert.Equal(
            2,
            await dbContext.HabitCompletions.CountAsync());

        Assert.Contains(
            dbContext.HabitCompletions,
            completion =>
                completion.CompletedDate
                    == new DateOnly(2026, 7, 18));

        Assert.Contains(
            dbContext.HabitCompletions,
            completion =>
                completion.CompletedDate
                    == new DateOnly(2026, 7, 19));
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

    private sealed class FixedTimeProvider : TimeProvider
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
