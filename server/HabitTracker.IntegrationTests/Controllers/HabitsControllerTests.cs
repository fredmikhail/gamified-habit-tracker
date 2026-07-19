using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.DTOs;
using HabitTracker.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HabitTracker.IntegrationTests.Controllers;

public sealed class HabitsControllerTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public HabitsControllerTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;

        _jsonOptions =
            factory.Services
                .GetRequiredService<IOptions<JsonOptions>>()
                .Value
                .JsonSerializerOptions;
    }

    [Fact]
    public async Task CreateHabit_WhenAnonymous_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var request = CreateValidHabitRequest();

        var response =
            await client.PostAsJsonAsync(
                "/api/habits",
                request,
                _jsonOptions);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateHabit_WithoutCsrfToken_ReturnsBadRequest()
    {
        using var client = CreateClient();

        await RegisterAsync(client);

        var request = CreateValidHabitRequest();

        var response =
            await client.PostAsJsonAsync(
                "/api/habits",
                request,
                _jsonOptions);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateHabit_WithValidRequest_ReturnsCreatedHabit()
    {
        using var client = CreateClient();

        await RegisterAsync(client);

        var csrfToken =
            await GetCsrfTokenAsync(client);

        var createRequest =
            new CreateHabitRequest
            {
                Name = "  Go to gym  ",
                Description =
                    "  Complete a planned gym session.  ",
                Category = "  Fitness  ",
                FrequencyType =
                    HabitFrequencyType.Weekly,
                TargetCount = 3,
                Difficulty =
                    HabitDifficulty.Elite
            };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/habits")
            {
                Content =
                    JsonContent.Create(
                        createRequest,
                        options: _jsonOptions)
            };

        request.Headers.Add(
            "X-CSRF-TOKEN",
            csrfToken);

        var response =
            await client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var responseBody =
            await response.Content
                .ReadFromJsonAsync<HabitResponse>(
                    _jsonOptions);

        Assert.NotNull(responseBody);

        Assert.NotEqual(
            Guid.Empty,
            responseBody.Id);

        Assert.Equal(
            "Go to gym",
            responseBody.Name);

        Assert.Equal(
            "Complete a planned gym session.",
            responseBody.Description);

        Assert.Equal(
            "Fitness",
            responseBody.Category);

        Assert.Equal(
            HabitFrequencyType.Weekly,
            responseBody.FrequencyType);

        Assert.Equal(
            3,
            responseBody.TargetCount);

        Assert.Equal(
            HabitDifficulty.Elite,
            responseBody.Difficulty);

        Assert.True(responseBody.IsActive);

        Assert.NotEqual(
            default,
            responseBody.CreatedAtUtc);

        Assert.Equal(
            responseBody.CreatedAtUtc,
            responseBody.UpdatedAtUtc);
    }

    [Fact]
    public async Task CreateHabit_WhenNameIsWhitespace_ReturnsValidationProblemDetails()
    {
        using var client = CreateClient();

        await RegisterAsync(client);

        var csrfToken =
            await GetCsrfTokenAsync(client);

        var createRequest =
            CreateValidHabitRequest();

        createRequest.Name = "   ";

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/habits")
            {
                Content =
                    JsonContent.Create(
                        createRequest,
                        options: _jsonOptions)
            };

        request.Headers.Add(
            "X-CSRF-TOKEN",
            csrfToken);

        var response =
            await client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problemDetails =
            await response.Content
                .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problemDetails);

        Assert.Equal(
            StatusCodes.Status400BadRequest,
            problemDetails.Status);

        Assert.Equal(
            "One or more validation errors occurred.",
            problemDetails.Title);

        var nameError =
            Assert.Single(
                problemDetails.Errors,
                error =>
                    string.Equals(
                        error.Key,
                        nameof(CreateHabitRequest.Name),
                        StringComparison.OrdinalIgnoreCase));

        Assert.NotEmpty(nameError.Value);
    }

    [Fact]
    public async Task CreateHabit_WhenDailyTargetCountIsNotOne_ReturnsBadRequestProblemDetails()
    {
        using var client = CreateClient();

        await RegisterAsync(client);

        var csrfToken =
            await GetCsrfTokenAsync(client);

        var createRequest =
            CreateValidHabitRequest();

        createRequest.FrequencyType =
            HabitFrequencyType.Daily;

        createRequest.TargetCount = 2;

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/habits")
            {
                Content =
                    JsonContent.Create(
                        createRequest,
                        options: _jsonOptions)
            };

        request.Headers.Add(
            "X-CSRF-TOKEN",
            csrfToken);

        var response =
            await client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problemDetails =
            await response.Content
                .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problemDetails);

        Assert.Equal(
            "Invalid habit target count",
            problemDetails.Title);

        Assert.Equal(
            "Daily habits must have a target count of 1. "
            + "Weekly habits must have a target count between 1 and 7.",
            problemDetails.Detail);

        Assert.Equal(
            "/api/habits",
            problemDetails.Instance);
    }

    [Fact]
    public async Task GetUserHabits_WhenAnonymous_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response =
            await client.GetAsync("/api/habits");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetUserHabits_WhenAuthenticated_ReturnsOnlyOwnedActiveHabitsNewestFirst()
    {
        using var client = CreateClient();

        var registration =
            await RegisterAsync(client);

        var otherUserRegistration =
            await RegisterAsync(CreateClient());

        var baseTimestampUtc =
            DateTime.UtcNow.AddDays(-1);

        var olderActiveHabit =
            await SeedHabitAsync(
                registration.User.Id,
                "Older active habit",
                baseTimestampUtc,
                isActive: true);

        var newerActiveHabit =
            await SeedHabitAsync(
                registration.User.Id,
                "Newer active habit",
                baseTimestampUtc.AddHours(1),
                isActive: true);

        await SeedHabitAsync(
            registration.User.Id,
            "Inactive habit",
            baseTimestampUtc.AddHours(2),
            isActive: false);

        await SeedHabitAsync(
            otherUserRegistration.User.Id,
            "Another user's habit",
            baseTimestampUtc.AddHours(3),
            isActive: true);

        var response =
            await client.GetAsync("/api/habits");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var responseBody =
            await response.Content
                .ReadFromJsonAsync<List<HabitResponse>>(
                    _jsonOptions);

        Assert.NotNull(responseBody);

        Assert.Collection(
            responseBody,
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
    public async Task GetUserHabits_WhenInactiveHabitsAreIncluded_ReturnsActiveHabitsBeforeInactiveHabits()
    {
        using var client = CreateClient();

        var registration =
            await RegisterAsync(client);

        var baseTimestampUtc =
            DateTime.UtcNow.AddDays(-1);

        var olderActiveHabit =
            await SeedHabitAsync(
                registration.User.Id,
                "Older active habit",
                baseTimestampUtc,
                isActive: true);

        var newerActiveHabit =
            await SeedHabitAsync(
                registration.User.Id,
                "Newer active habit",
                baseTimestampUtc.AddHours(1),
                isActive: true);

        var olderInactiveHabit =
            await SeedHabitAsync(
                registration.User.Id,
                "Older inactive habit",
                baseTimestampUtc.AddHours(2),
                isActive: false);

        var newerInactiveHabit =
            await SeedHabitAsync(
                registration.User.Id,
                "Newer inactive habit",
                baseTimestampUtc.AddHours(3),
                isActive: false);

        var response =
            await client.GetAsync(
                "/api/habits?includeInactive=true");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var responseBody =
            await response.Content
                .ReadFromJsonAsync<List<HabitResponse>>(
                    _jsonOptions);

        Assert.NotNull(responseBody);

        Assert.Collection(
            responseBody,
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
    public async Task GetUserHabit_WhenHabitBelongsToUser_ReturnsHabit()
    {
        using var client = CreateClient();

        var registration =
            await RegisterAsync(client);

        var habit =
            await SeedHabitAsync(
                registration.User.Id,
                "Read C# textbook",
                DateTime.UtcNow,
                isActive: false);

        var response =
            await client.GetAsync(
                $"/api/habits/{habit.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var responseBody =
            await response.Content
                .ReadFromJsonAsync<HabitResponse>(
                    _jsonOptions);

        Assert.NotNull(responseBody);
        Assert.Equal(habit.Id, responseBody.Id);
        Assert.Equal(habit.Name, responseBody.Name);
        Assert.False(responseBody.IsActive);
    }

    [Fact]
    public async Task GetUserHabit_WhenHabitBelongsToAnotherUser_ReturnsNotFound()
    {
        using var ownerClient = CreateClient();
        using var requestingClient = CreateClient();

        var ownerRegistration =
            await RegisterAsync(ownerClient);

        await RegisterAsync(requestingClient);

        var habit =
            await SeedHabitAsync(
                ownerRegistration.User.Id,
                "Private habit",
                DateTime.UtcNow,
                isActive: true);

        var response =
            await requestingClient.GetAsync(
                $"/api/habits/{habit.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task GetUserHabit_WhenHabitDoesNotExist_ReturnsNotFound()
    {
        using var client = CreateClient();

        await RegisterAsync(client);

        var missingHabitId =
            Guid.CreateVersion7();

        var response =
            await client.GetAsync(
                $"/api/habits/{missingHabitId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true
            });
    }

    private static async Task<AuthResponse> RegisterAsync(
    HttpClient client)
    {
        var csrfToken =
            await GetCsrfTokenAsync(client);

        var uniqueSuffix =
            Guid.NewGuid()
                .ToString("N");

        var registerRequest =
            new RegisterRequest
            {
                Email =
                    $"user_{uniqueSuffix}@example.com",
                Username =
                    $"user_{uniqueSuffix[..8]}",
                Password =
                    "StrongPassword123!",
                TimeZone =
                    "America/Toronto"
            };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/auth/register")
            {
                Content =
                    JsonContent.Create(registerRequest)
            };

        request.Headers.Add(
            "X-CSRF-TOKEN",
            csrfToken);

        var response =
            await client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var responseBody =
            await response.Content
                .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(responseBody);

        return responseBody;
    }

    private async Task<Habit> SeedHabitAsync(
    Guid userId,
    string name,
    DateTime createdAtUtc,
    bool isActive)
    {
        using var scope =
            _factory.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        var habit = new Habit
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

        dbContext.Habits.Add(habit);

        await dbContext.SaveChangesAsync();

        return habit;
    }

    private static async Task<string> GetCsrfTokenAsync(
        HttpClient client)
    {
        var response =
            await client.GetAsync(
                "/api/auth/csrf-token");

        response.EnsureSuccessStatusCode();

        var responseBody =
            await response.Content
                .ReadFromJsonAsync<AntiforgeryTokenResponse>();

        Assert.NotNull(responseBody);

        Assert.False(
            string.IsNullOrWhiteSpace(
                responseBody.RequestToken));

        return responseBody.RequestToken;
    }

    private static CreateHabitRequest CreateValidHabitRequest()
    {
        return new CreateHabitRequest
        {
            Name = "Read C# textbook",
            Description = "Read one chapter.",
            Category = "Learning",
            FrequencyType =
                HabitFrequencyType.Daily,
            TargetCount = 1,
            Difficulty =
                HabitDifficulty.Medium
        };
    }
}