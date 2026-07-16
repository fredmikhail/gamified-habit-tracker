using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using HabitTracker.Api.DTOs;
using HabitTracker.Api.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HabitTracker.Api.Services;

public sealed class AuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(
        AppDbContext dbContext,
        IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidIanaTimeZone(request.TimeZone))
        {
            throw new InvalidIanaTimeZoneException();
        }

        var email = request.Email.Trim();
        var username = request.Username.Trim();

        var normalizedEmail =
            NormalizeAccountIdentifier(email);

        var normalizedUsername =
            NormalizeAccountIdentifier(username);

        var accountAlreadyExists =
            await _dbContext.Users.AnyAsync(
                user =>
                    user.NormalizedEmail == normalizedEmail
                    || user.NormalizedUsername == normalizedUsername,
                cancellationToken);

        if (accountAlreadyExists)
        {
            throw new AccountConflictException();
        }

        var createdAtUtc = DateTime.UtcNow;

        var user = new User
        {
            Email = email,
            NormalizedEmail = normalizedEmail,
            Username = username,
            NormalizedUsername = normalizedUsername,
            CreatedAtUtc = createdAtUtc
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                request.Password);

        var userSettings = new UserSettings
        {
            UserId = user.Id,
            DisplayName = username,
            TimeZone = request.TimeZone,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc,
            User = user
        };

        user.UserSettings = userSettings;

        _dbContext.Users.Add(user);

        try
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateException exception)
            when (
                exception.InnerException
                    is PostgresException postgresException
                && postgresException.SqlState
                    == PostgresErrorCodes.UniqueViolation
                && postgresException.ConstraintName
                    is "ix_users_normalized_email"
                        or "ix_users_normalized_username")
        {
            throw new AccountConflictException();
        }

        return new AuthResponse
        {
            User = CreateCurrentUserResponse(user)
        };
    }

    public async Task<AuthResponse> LoginAsync(
    LoginRequest request,
    CancellationToken cancellationToken = default)
    {
        var normalizedEmail =
            NormalizeAccountIdentifier(request.Email);

        var user = await _dbContext.Users
            .Include(user => user.UserSettings)
            .SingleOrDefaultAsync(
                user => user.NormalizedEmail == normalizedEmail,
                cancellationToken);

        if (user is null)
        {
            throw new InvalidCredentialsException();
        }

        var passwordVerificationResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

        if (passwordVerificationResult
            == PasswordVerificationResult.Failed)
        {
            throw new InvalidCredentialsException();
        }

        if (passwordVerificationResult
            == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    request.Password);
        }

        user.LastLoginAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new AuthResponse
        {
            User = CreateCurrentUserResponse(user)
        };
    }

    public async Task<CurrentUserResponse?> GetCurrentUserAsync(
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .Include(user => user.UserSettings)
            .SingleOrDefaultAsync(
                user => user.Id == userId,
                cancellationToken);

        if (user is null)
        {
            return null;
        }

        return CreateCurrentUserResponse(user);
    }

    private static string NormalizeAccountIdentifier(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static bool IsValidIanaTimeZone(string timeZone)
    {
        return TimeZoneInfo.TryConvertIanaIdToWindowsId(
            timeZone,
            out _);
    }

    private static CurrentUserResponse CreateCurrentUserResponse(
        User user)
    {
        return new CurrentUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            DisplayName = user.UserSettings.DisplayName,
            TimeZone = user.UserSettings.TimeZone
        };
    }
}
