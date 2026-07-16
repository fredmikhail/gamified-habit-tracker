using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using HabitTracker.Api.DTOs;
using HabitTracker.Api.Exceptions;
using HabitTracker.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Tests.Services;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_WhenTimeZoneIsInvalid_ThrowsInvalidIanaTimeZoneException()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AppDbContext(options);

        var passwordHasher = new PasswordHasher<User>();
        var authService = new AuthService(
            dbContext,
            passwordHasher);

        var request = new RegisterRequest
        {
            Email = "fred@example.com",
            Username = "fred",
            Password = "a-secure-password-with-spaces",
            TimeZone = "Dragon-Castle/Moon-Time"
        };

        await Assert.ThrowsAsync<InvalidIanaTimeZoneException>(
            () => authService.RegisterAsync(request));

        Assert.Empty(dbContext.Users);
        Assert.Empty(dbContext.UserSettings);
    }

    [Fact]
    public async Task RegisterAsync_WhenRequestIsValid_CreatesUserAndSettings()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AppDbContext(options);

        var passwordHasher = new PasswordHasher<User>();

        var authService = new AuthService(
            dbContext,
            passwordHasher);

        var request = new RegisterRequest
        {
            Email = "  Fred@Example.com  ",
            Username = "  Fred_95  ",
            Password = "a-secure-password-with-spaces",
            TimeZone = "America/Toronto"
        };

        var response = await authService.RegisterAsync(request);

        var savedUser = Assert.Single(dbContext.Users);
        var savedSettings = Assert.Single(dbContext.UserSettings);

        Assert.Equal("Fred@Example.com", savedUser.Email);
        Assert.Equal("FRED@EXAMPLE.COM", savedUser.NormalizedEmail);
        Assert.Equal("Fred_95", savedUser.Username);
        Assert.Equal("FRED_95", savedUser.NormalizedUsername);

        Assert.NotEqual(request.Password, savedUser.PasswordHash);

        var passwordVerificationResult =
            passwordHasher.VerifyHashedPassword(
                savedUser,
                savedUser.PasswordHash,
                request.Password);

        Assert.NotEqual(
            PasswordVerificationResult.Failed,
            passwordVerificationResult);

        Assert.Equal(savedUser.Id, savedSettings.UserId);
        Assert.Equal("Fred_95", savedSettings.DisplayName);
        Assert.Equal("America/Toronto", savedSettings.TimeZone);
        Assert.Equal(savedUser.CreatedAtUtc, savedSettings.CreatedAtUtc);
        Assert.Equal(savedSettings.CreatedAtUtc, savedSettings.UpdatedAtUtc);

        Assert.Equal(savedUser.Id, response.User.Id);
        Assert.Equal(savedUser.Email, response.User.Email);
        Assert.Equal(savedUser.Username, response.User.Username);
        Assert.Equal(savedSettings.DisplayName, response.User.DisplayName);
        Assert.Equal(savedSettings.TimeZone, response.User.TimeZone);
    }

    [Fact]
    public async Task RegisterAsync_WhenNormalizedEmailAlreadyExists_ThrowsAccountConflictException()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AppDbContext(options);

        var createdAtUtc = DateTime.UtcNow;

        var existingUser = new User
        {
            Email = "fred@example.com",
            NormalizedEmail = "FRED@EXAMPLE.COM",
            Username = "fred",
            NormalizedUsername = "FRED",
            PasswordHash = "existing-password-hash",
            CreatedAtUtc = createdAtUtc
        };

        var existingSettings = new UserSettings
        {
            UserId = existingUser.Id,
            DisplayName = "fred",
            TimeZone = "America/Toronto",
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc,
            User = existingUser
        };

        existingUser.UserSettings = existingSettings;

        dbContext.Users.Add(existingUser);
        await dbContext.SaveChangesAsync();

        var passwordHasher = new PasswordHasher<User>();

        var authService = new AuthService(
            dbContext,
            passwordHasher);

        var request = new RegisterRequest
        {
            Email = "  FRED@Example.com  ",
            Username = "another_user",
            Password = "another-secure-password",
            TimeZone = "America/Toronto"
        };

        await Assert.ThrowsAsync<AccountConflictException>(
            () => authService.RegisterAsync(request));

        Assert.Single(dbContext.Users);
        Assert.Single(dbContext.UserSettings);
    }

    [Fact]
    public async Task RegisterAsync_WhenNormalizedUsernameAlreadyExists_ThrowsAccountConflictException()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AppDbContext(options);

        var createdAtUtc = DateTime.UtcNow;

        var existingUser = new User
        {
            Email = "fred@example.com",
            NormalizedEmail = "FRED@EXAMPLE.COM",
            Username = "Fred_95",
            NormalizedUsername = "FRED_95",
            PasswordHash = "existing-password-hash",
            CreatedAtUtc = createdAtUtc
        };

        var existingSettings = new UserSettings
        {
            UserId = existingUser.Id,
            DisplayName = "Fred_95",
            TimeZone = "America/Toronto",
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc,
            User = existingUser
        };

        existingUser.UserSettings = existingSettings;

        dbContext.Users.Add(existingUser);
        await dbContext.SaveChangesAsync();

        var passwordHasher = new PasswordHasher<User>();

        var authService = new AuthService(
            dbContext,
            passwordHasher);

        var request = new RegisterRequest
        {
            Email = "another@example.com",
            Username = "  fred_95  ",
            Password = "another-secure-password",
            TimeZone = "America/Toronto"
        };

        await Assert.ThrowsAsync<AccountConflictException>(
            () => authService.RegisterAsync(request));

        Assert.Single(dbContext.Users);
        Assert.Single(dbContext.UserSettings);
    }
}
