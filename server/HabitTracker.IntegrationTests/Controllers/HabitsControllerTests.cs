using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true
            });
    }

    private static async Task RegisterAsync(
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