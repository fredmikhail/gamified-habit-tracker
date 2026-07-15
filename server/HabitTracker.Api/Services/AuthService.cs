using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using Microsoft.AspNetCore.Identity;

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
}
