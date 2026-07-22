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
    private static readonly DateTimeOffset UtcNow =
        new(2026, 7, 20, 2, 30, 0, TimeSpan.Zero);

    private static readonly DateOnly CurrentLocalDate =
        new(2026, 7, 19);

    [Fact]
    public async Task CompleteHabitAsync_WhenRequestIsValid_CreatesCompletionUsingUserLocalDate()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();
        var habitId = Guid.CreateVersion7();

        AddUserWithSettings(dbContext, userId, UtcNow.UtcDateTime);

        var (habit, configuration) =
            CreateHabitWithConfiguration(
                userId,
                habitId,
                UtcNow.UtcDateTime.AddDays(-1),
                isActive: true,
                effectiveFromDate: CurrentLocalDate.AddDays(-1));

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var completionService =
            CreateCompletionService(
                dbContext,
                new FixedTimeProvider(UtcNow));

        var response =
            await completionService.CompleteHabitAsync(
                userId,
                habitId,
                new CompleteHabitRequest
                {
                    Notes = "  Strong workout today.  "
                });

        Assert.NotNull(response);

        var savedCompletion =
            Assert.Single(dbContext.HabitCompletions);

        Assert.NotEqual(Guid.Empty, savedCompletion.Id);
        Assert.Equal(userId, savedCompletion.UserId);
        Assert.Equal(habitId, savedCompletion.HabitId);

        Assert.Equal(
            configuration.Id,
            savedCompletion.HabitConfigurationVersionId);

        Assert.Equal(CurrentLocalDate, savedCompletion.CompletedDate);
        Assert.Equal(UtcNow.UtcDateTime, savedCompletion.CompletedAtUtc);
        Assert.Equal("Strong workout today.", savedCompletion.Notes);

        Assert.Equal(savedCompletion.Id, response.Completion.Id);
        Assert.Equal(savedCompletion.HabitId, response.Completion.HabitId);
        Assert.Equal(
            savedCompletion.CompletedDate,
            response.Completion.CompletedDate);
        Assert.Equal(
            savedCompletion.CompletedAtUtc,
            response.Completion.CompletedAtUtc);
        Assert.Equal(savedCompletion.Notes, response.Completion.Notes);
    }

    [Fact]
    public async Task CompleteHabitAsync_WhenNotesAreBlank_StoresNull()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();
        var habitId = Guid.CreateVersion7();

        AddUserWithSettings(dbContext, userId, UtcNow.UtcDateTime);

        var (habit, _) =
            CreateHabitWithConfiguration(
                userId,
                habitId,
                UtcNow.UtcDateTime.AddDays(-1),
                isActive: true,
                effectiveFromDate: CurrentLocalDate.AddDays(-1));

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var completionService =
            CreateCompletionService(
                dbContext,
                new FixedTimeProvider(UtcNow));

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
    public async Task CompleteHabitAsync_WhenConfigurationChanges_UsesVersionEffectiveOnCompletionDate()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();
        var habitId = Guid.CreateVersion7();

        AddUserWithSettings(dbContext, userId, UtcNow.UtcDateTime);

        var habit = new Habit
        {
            Id = habitId,
            UserId = userId,
            Name = "Study",
            Category = HabitCategory.GeneralGrowth,
            FrequencyType = HabitFrequencyType.Daily,
            TargetCount = 1,
            Difficulty = HabitDifficulty.Easy,
            IsActive = true,
            CreatedAtUtc = UtcNow.UtcDateTime.AddDays(-30),
            UpdatedAtUtc = UtcNow.UtcDateTime.AddDays(-1)
        };

        var previousConfiguration =
            new HabitConfigurationVersion
            {
                HabitId = habitId,
                VersionNumber = 1,
                Category = HabitCategory.GeneralGrowth,
                FrequencyType = HabitFrequencyType.Daily,
                TargetCount = 1,
                Difficulty = HabitDifficulty.Easy,
                EffectiveFromDate = CurrentLocalDate.AddDays(-30),
                EffectiveToDateExclusive = CurrentLocalDate,
                CreatedAtUtc = UtcNow.UtcDateTime.AddDays(-30)
            };

        var effectiveConfiguration =
            new HabitConfigurationVersion
            {
                HabitId = habitId,
                VersionNumber = 2,
                Category = HabitCategory.LearningAndSkills,
                FrequencyType = HabitFrequencyType.Daily,
                TargetCount = 1,
                Difficulty = HabitDifficulty.Elite,
                EffectiveFromDate = CurrentLocalDate,
                CreatedAtUtc = UtcNow.UtcDateTime.AddDays(-1)
            };

        habit.HabitConfigurationVersions.Add(previousConfiguration);
        habit.HabitConfigurationVersions.Add(effectiveConfiguration);

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var completionService =
            CreateCompletionService(
                dbContext,
                new FixedTimeProvider(UtcNow));

        var response =
            await completionService.CompleteHabitAsync(
                userId,
                habitId,
                new CompleteHabitRequest());

        Assert.NotNull(response);

        var savedCompletion =
            Assert.Single(dbContext.HabitCompletions);

        Assert.Equal(
            effectiveConfiguration.Id,
            savedCompletion.HabitConfigurationVersionId);

        Assert.Collection(
            response.Rewards,
            primaryReward =>
            {
                Assert.Equal(
                    AttributeType.Mind,
                    primaryReward.AttributeType);
                Assert.Equal(35, primaryReward.XpAmount);
            },
            secondaryReward =>
            {
                Assert.Equal(
                    AttributeType.Focus,
                    secondaryReward.AttributeType);
                Assert.Equal(15, secondaryReward.XpAmount);
            });
    }

    [Fact]
    public async Task CompleteHabitAsync_WhenHabitDoesNotExist_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(dbContext, userId, UtcNow.UtcDateTime);
        await dbContext.SaveChangesAsync();

        var completionService =
            CreateCompletionService(
                dbContext,
                new FixedTimeProvider(UtcNow));

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

        var ownerUserId = Guid.CreateVersion7();
        var requestingUserId = Guid.CreateVersion7();

        var (habit, _) =
            CreateHabitWithConfiguration(
                ownerUserId,
                Guid.CreateVersion7(),
                UtcNow.UtcDateTime.AddDays(-1),
                isActive: true,
                effectiveFromDate: CurrentLocalDate.AddDays(-1));

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var completionService =
            CreateCompletionService(
                dbContext,
                new FixedTimeProvider(UtcNow));

        var response =
            await completionService.CompleteHabitAsync(
                requestingUserId,
                habit.Id,
                new CompleteHabitRequest());

        Assert.Null(response);
        Assert.Empty(dbContext.HabitCompletions);
    }

    [Fact]
    public async Task CompleteHabitAsync_WhenHabitIsInactive_ThrowsInactiveHabitException()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(dbContext, userId, UtcNow.UtcDateTime);

        var (habit, _) =
            CreateHabitWithConfiguration(
                userId,
                Guid.CreateVersion7(),
                UtcNow.UtcDateTime.AddDays(-1),
                isActive: false,
                effectiveFromDate: CurrentLocalDate.AddDays(-1));

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        var completionService =
            CreateCompletionService(
                dbContext,
                new FixedTimeProvider(UtcNow));

        await Assert.ThrowsAsync<InactiveHabitException>(
            () =>
                completionService.CompleteHabitAsync(
                    userId,
                    habit.Id,
                    new CompleteHabitRequest()));

        Assert.Empty(dbContext.HabitCompletions);
    }

    [Fact]
    public async Task CompleteHabitAsync_WhenHabitIsAlreadyCompletedToday_ThrowsHabitAlreadyCompletedException()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(dbContext, userId, UtcNow.UtcDateTime);

        var (habit, configuration) =
            CreateHabitWithConfiguration(
                userId,
                Guid.CreateVersion7(),
                UtcNow.UtcDateTime.AddDays(-1),
                isActive: true,
                effectiveFromDate: CurrentLocalDate.AddDays(-1));

        dbContext.Habits.Add(habit);

        dbContext.HabitCompletions.Add(
            CreateCompletion(
                userId,
                habit,
                configuration,
                CurrentLocalDate,
                UtcNow.UtcDateTime.AddHours(-1)));

        await dbContext.SaveChangesAsync();

        var completionService =
            CreateCompletionService(
                dbContext,
                new FixedTimeProvider(UtcNow));

        await Assert.ThrowsAsync<HabitAlreadyCompletedException>(
            () =>
                completionService.CompleteHabitAsync(
                    userId,
                    habit.Id,
                    new CompleteHabitRequest()));

        Assert.Single(dbContext.HabitCompletions);
    }

    [Fact]
    public async Task CompleteHabitAsync_WhenTodaysCompletionWasUndone_CreatesNewActiveCompletion()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(
            dbContext,
            userId,
            UtcNow.UtcDateTime);

        var (habit, configuration) =
            CreateHabitWithConfiguration(
                userId,
                Guid.CreateVersion7(),
                UtcNow.UtcDateTime.AddDays(-1),
                isActive: true,
                effectiveFromDate:
                    CurrentLocalDate.AddDays(-1));

        var undoneCompletion =
            CreateCompletion(
                userId,
                habit,
                configuration,
                CurrentLocalDate,
                UtcNow.UtcDateTime.AddHours(-1));

        undoneCompletion.UndoneAtUtc =
            UtcNow.UtcDateTime.AddMinutes(-15);

        dbContext.Habits.Add(habit);

        dbContext.HabitCompletions.Add(
            undoneCompletion);

        await dbContext.SaveChangesAsync();

        var completionService =
            CreateCompletionService(
                dbContext,
                new FixedTimeProvider(UtcNow));

        var response =
            await completionService.CompleteHabitAsync(
                userId,
                habit.Id,
                new CompleteHabitRequest());

        Assert.NotNull(response);

        var completions =
            await dbContext.HabitCompletions
                .Where(completion =>
                    completion.HabitId == habit.Id)
                .ToListAsync();

        Assert.Equal(2, completions.Count);

        Assert.Single(
            completions,
            completion =>
                completion.UndoneAtUtc is not null);

        var activeCompletion =
            Assert.Single(
                completions,
                completion =>
                    completion.UndoneAtUtc is null);

        Assert.Equal(
            response.Completion.Id,
            activeCompletion.Id);
    }

    [Fact]
    public async Task CompleteHabitAsync_WhenHabitWasCompletedYesterday_CreatesTodaysCompletion()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(dbContext, userId, UtcNow.UtcDateTime);

        var (habit, configuration) =
            CreateHabitWithConfiguration(
                userId,
                Guid.CreateVersion7(),
                UtcNow.UtcDateTime.AddDays(-2),
                isActive: true,
                effectiveFromDate: CurrentLocalDate.AddDays(-2));

        dbContext.Habits.Add(habit);

        dbContext.HabitCompletions.Add(
            CreateCompletion(
                userId,
                habit,
                configuration,
                CurrentLocalDate.AddDays(-1),
                UtcNow.UtcDateTime.AddDays(-1)));

        await dbContext.SaveChangesAsync();

        var completionService =
            CreateCompletionService(
                dbContext,
                new FixedTimeProvider(UtcNow));

        var response =
            await completionService.CompleteHabitAsync(
                userId,
                habit.Id,
                new CompleteHabitRequest());

        Assert.NotNull(response);
        Assert.Equal(
            CurrentLocalDate,
            response.Completion.CompletedDate);

        Assert.Equal(
            2,
            await dbContext.HabitCompletions.CountAsync());

        Assert.Contains(
            dbContext.HabitCompletions,
            completion =>
                completion.CompletedDate
                    == CurrentLocalDate.AddDays(-1));

        Assert.Contains(
            dbContext.HabitCompletions,
            completion =>
                completion.CompletedDate
                    == CurrentLocalDate);
    }

    [Fact]
    public async Task UndoTodayAsync_WhenCompletionExistsToday_MarksCompletionUndoneAndKeepsHabit()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(dbContext, userId, UtcNow.UtcDateTime);

        var (habit, configuration) =
            CreateHabitWithConfiguration(
                userId,
                Guid.CreateVersion7(),
                UtcNow.UtcDateTime.AddDays(-2),
                isActive: true,
                effectiveFromDate: CurrentLocalDate.AddDays(-2));

        dbContext.Habits.Add(habit);

        dbContext.HabitCompletions.Add(
            CreateCompletion(
                userId,
                habit,
                configuration,
                CurrentLocalDate,
                UtcNow.UtcDateTime.AddHours(-1)));

        await dbContext.SaveChangesAsync();

        var completionService =
            CreateCompletionService(
                dbContext,
                new FixedTimeProvider(UtcNow));

        var wasRemoved =
            await completionService.UndoTodayAsync(
                userId,
                habit.Id);

        Assert.True(wasRemoved);

        var savedCompletion =
    Assert.Single(
        dbContext.HabitCompletions,
        completion =>
            completion.HabitId == habit.Id);

        Assert.Equal(
            UtcNow.UtcDateTime,
            savedCompletion.UndoneAtUtc);

        var savedHabit =
            Assert.Single(
                dbContext.Habits,
                candidate =>
                    candidate.Id == habit.Id);

        Assert.True(savedHabit.IsActive);
    }

    [Fact]
    public async Task UndoTodayAsync_WhenHabitIsInactive_MarksCompletionUndone()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(dbContext, userId, UtcNow.UtcDateTime);

        var (habit, configuration) =
            CreateHabitWithConfiguration(
                userId,
                Guid.CreateVersion7(),
                UtcNow.UtcDateTime.AddDays(-2),
                isActive: false,
                effectiveFromDate: CurrentLocalDate.AddDays(-2));

        dbContext.Habits.Add(habit);

        dbContext.HabitCompletions.Add(
            CreateCompletion(
                userId,
                habit,
                configuration,
                CurrentLocalDate,
                UtcNow.UtcDateTime.AddHours(-1)));

        await dbContext.SaveChangesAsync();

        var completionService =
            CreateCompletionService(
                dbContext,
                new FixedTimeProvider(UtcNow));

        var wasRemoved =
            await completionService.UndoTodayAsync(
                userId,
                habit.Id);

        Assert.True(wasRemoved);

        var savedCompletion =
    Assert.Single(
        dbContext.HabitCompletions,
        completion =>
            completion.HabitId == habit.Id);

        Assert.Equal(
            UtcNow.UtcDateTime,
            savedCompletion.UndoneAtUtc);

        var savedHabit =
            Assert.Single(
                dbContext.Habits,
                candidate =>
                    candidate.Id == habit.Id);

        Assert.False(savedHabit.IsActive);
    }

    [Fact]
    public async Task UndoTodayAsync_WhenHabitDoesNotExist_ReturnsFalse()
    {
        await using var dbContext = CreateDbContext();

        var completionService =
            CreateCompletionService(
                dbContext,
                new FixedTimeProvider(UtcNow));

        var wasRemoved =
            await completionService.UndoTodayAsync(
                Guid.CreateVersion7(),
                Guid.CreateVersion7());

        Assert.False(wasRemoved);
        Assert.Empty(dbContext.HabitCompletions);
    }

    [Fact]
    public async Task UndoTodayAsync_WhenHabitBelongsToAnotherUser_ReturnsFalseAndKeepsCompletion()
    {
        await using var dbContext = CreateDbContext();

        var ownerId = Guid.CreateVersion7();
        var requestingUserId = Guid.CreateVersion7();

        var (habit, configuration) =
            CreateHabitWithConfiguration(
                ownerId,
                Guid.CreateVersion7(),
                UtcNow.UtcDateTime.AddDays(-2),
                isActive: true,
                effectiveFromDate: CurrentLocalDate.AddDays(-2));

        dbContext.Habits.Add(habit);

        dbContext.HabitCompletions.Add(
            CreateCompletion(
                ownerId,
                habit,
                configuration,
                CurrentLocalDate,
                UtcNow.UtcDateTime.AddHours(-1)));

        await dbContext.SaveChangesAsync();

        var completionService =
            CreateCompletionService(
                dbContext,
                new FixedTimeProvider(UtcNow));

        var wasRemoved =
            await completionService.UndoTodayAsync(
                requestingUserId,
                habit.Id);

        Assert.False(wasRemoved);

        Assert.Single(
            dbContext.HabitCompletions,
            completion =>
                completion.HabitId == habit.Id);
    }

    [Fact]
    public async Task UndoTodayAsync_WhenOnlyYesterdayCompletionExists_ReturnsFalseAndKeepsCompletion()
    {
        await using var dbContext = CreateDbContext();

        var userId = Guid.CreateVersion7();

        AddUserWithSettings(dbContext, userId, UtcNow.UtcDateTime);

        var (habit, configuration) =
            CreateHabitWithConfiguration(
                userId,
                Guid.CreateVersion7(),
                UtcNow.UtcDateTime.AddDays(-2),
                isActive: true,
                effectiveFromDate: CurrentLocalDate.AddDays(-2));

        dbContext.Habits.Add(habit);

        dbContext.HabitCompletions.Add(
            CreateCompletion(
                userId,
                habit,
                configuration,
                CurrentLocalDate.AddDays(-1),
                UtcNow.UtcDateTime.AddDays(-1)));

        await dbContext.SaveChangesAsync();

        var completionService =
            CreateCompletionService(
                dbContext,
                new FixedTimeProvider(UtcNow));

        var wasRemoved =
            await completionService.UndoTodayAsync(
                userId,
                habit.Id);

        Assert.False(wasRemoved);

        var remainingCompletion =
            Assert.Single(
                dbContext.HabitCompletions,
                completion =>
                    completion.HabitId == habit.Id);

        Assert.Equal(
            CurrentLocalDate.AddDays(-1),
            remainingCompletion.CompletedDate);
    }

    private static CompletionService CreateCompletionService(
        AppDbContext dbContext,
        TimeProvider timeProvider)
    {
        var xpService = new XpService();

        return new CompletionService(
            dbContext,
            timeProvider,
            new AttributeService(
                dbContext,
                xpService),
            xpService);
    }

    private static void AddUserWithSettings(
        AppDbContext dbContext,
        Guid userId,
        DateTime timestampUtc)
    {
        var uniqueSuffix = userId.ToString("N");

        var user = new User
        {
            Id = userId,
            Email = $"user_{uniqueSuffix}@example.com",
            NormalizedEmail =
                $"USER_{uniqueSuffix}@EXAMPLE.COM",
            Username = $"user_{uniqueSuffix[..8]}",
            NormalizedUsername =
                $"USER_{uniqueSuffix[..8]}",
            PasswordHash = "test-password-hash",
            CreatedAtUtc = timestampUtc.AddDays(-1)
        };

        var settings = new UserSettings
        {
            UserId = userId,
            DisplayName = "Test User",
            TimeZone = "America/Toronto",
            CreatedAtUtc = timestampUtc.AddDays(-1),
            UpdatedAtUtc = timestampUtc.AddDays(-1),
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
            DateTime timestampUtc,
            bool isActive,
            DateOnly effectiveFromDate,
            HabitCategory category =
                HabitCategory.FitnessAndMovement,
            HabitDifficulty difficulty =
                HabitDifficulty.Medium)
    {
        var habit = new Habit
        {
            Id = habitId,
            UserId = userId,
            Name = "Go to gym",
            Category = category,
            FrequencyType = HabitFrequencyType.Daily,
            TargetCount = 1,
            Difficulty = difficulty,
            IsActive = isActive,
            CreatedAtUtc = timestampUtc,
            UpdatedAtUtc = timestampUtc
        };

        var configuration =
            new HabitConfigurationVersion
            {
                HabitId = habitId,
                VersionNumber = 1,
                Category = category,
                FrequencyType =
                    HabitFrequencyType.Daily,
                TargetCount = 1,
                Difficulty = difficulty,
                EffectiveFromDate = effectiveFromDate,
                CreatedAtUtc = timestampUtc
            };

        habit.HabitConfigurationVersions.Add(configuration);

        return (habit, configuration);
    }

    private static HabitCompletion CreateCompletion(
        Guid userId,
        Habit habit,
        HabitConfigurationVersion configuration,
        DateOnly completedDate,
        DateTime completedAtUtc)
    {
        return new HabitCompletion
        {
            UserId = userId,
            HabitId = habit.Id,
            Habit = habit,
            HabitConfigurationVersionId =
                configuration.Id,
            HabitConfigurationVersion =
                configuration,
            CompletedDate = completedDate,
            CompletedAtUtc = completedAtUtc
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
