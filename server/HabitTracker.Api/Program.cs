using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using HabitTracker.Api.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

const string frontendCorsPolicy = "FrontendCorsPolicy";

// Add services to the container.

builder.Services.AddControllers();

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "HabitTracker.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.Cookie.SecurePolicy =
            builder.Environment.IsDevelopment()
                ? CookieSecurePolicy.None
                : CookieSecurePolicy.Always;

        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = false;

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode =
                StatusCodes.Status403Forbidden;

            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(
            connectionString,
            npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable(
                    "__ef_migrations_history"))
        .UseSnakeCaseNamingConvention());

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext =
        scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!await dbContext.Database.CanConnectAsync())
    {
        throw new InvalidOperationException(
            "The application could not connect to the PostgreSQL database.");
    }

    app.Logger.LogInformation(
        "Successfully connected to PostgreSQL.");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(frontendCorsPolicy);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
