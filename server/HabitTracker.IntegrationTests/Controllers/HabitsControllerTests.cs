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

    [Fact]
    public async Task UpdateHabit_WhenAnonymous_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response =
            await PutHabitAsync(
                client,
                Guid.CreateVersion7(),
                CreateValidUpdateHabitRequest());

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task UpdateHabit_WithoutCsrfToken_ReturnsBadRequest()
    {
        using var client = CreateClient();

        var registration =
            await RegisterAsync(client);

        var habit =
            await SeedHabitAsync(
                registration.User.Id,
                "Existing habit",
                DateTime.UtcNow.AddDays(-1),
                isActive: true);

        var response =
            await PutHabitAsync(
                client,
                habit.Id,
                CreateValidUpdateHabitRequest());

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
    [Fact]
    public async Task UpdateHabit_WhenHabitBelongsToUser_UpdatesAndReturnsHabit()
    {
        using var client = CreateClient();

        var registration =
            await RegisterAsync(client);

        var createdAtUtc =
            DateTime.UtcNow.AddDays(-2);

        var habit =
            await SeedHabitAsync(
                registration.User.Id,
                "Old habit name",
                createdAtUtc,
                isActive: false);

        var csrfToken =
            await GetCsrfTokenAsync(client);

        var updateRequest =
            new UpdateHabitRequest
            {
                Name = "  Updated habit name  ",
                Description =
                    "  Updated description  ",
                Category = "  Fitness  ",
                FrequencyType =
                    HabitFrequencyType.Weekly,
                TargetCount = 4,
                Difficulty =
                    HabitDifficulty.Elite
            };

        var response =
            await PutHabitAsync(
                client,
                habit.Id,
                updateRequest,
                csrfToken);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var responseBody =
            await response.Content
                .ReadFromJsonAsync<HabitResponse>(
                    _jsonOptions);

        Assert.NotNull(responseBody);

        Assert.Equal(habit.Id, responseBody.Id);
        Assert.Equal(
            "Updated habit name",
            responseBody.Name);
        Assert.Equal(
            "Updated description",
            responseBody.Description);
        Assert.Equal(
            "Fitness",
            responseBody.Category);
        Assert.Equal(
            HabitFrequencyType.Weekly,
            responseBody.FrequencyType);
        Assert.Equal(4, responseBody.TargetCount);
        Assert.Equal(
            HabitDifficulty.Elite,
            responseBody.Difficulty);

        Assert.False(responseBody.IsActive);

        Assert.Equal(
            createdAtUtc,
            responseBody.CreatedAtUtc);

        Assert.True(
            responseBody.UpdatedAtUtc
                > createdAtUtc);

        var getResponse =
            await client.GetAsync(
                $"/api/habits/{habit.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        var savedHabit =
            await getResponse.Content
                .ReadFromJsonAsync<HabitResponse>(
                    _jsonOptions);

        Assert.NotNull(savedHabit);
        Assert.Equal(
            "Updated habit name",
            savedHabit.Name);
        Assert.False(savedHabit.IsActive);
    }

    [Fact]
    public async Task UpdateHabit_WhenNameIsWhitespace_ReturnsValidationProblemDetails()
    {
        using var client = CreateClient();

        var registration =
            await RegisterAsync(client);

        var habit =
            await SeedHabitAsync(
                registration.User.Id,
                "Existing habit",
                DateTime.UtcNow.AddDays(-1),
                isActive: true);

        var csrfToken =
            await GetCsrfTokenAsync(client);

        var updateRequest =
            CreateValidUpdateHabitRequest();

        updateRequest.Name = "   ";

        var response =
            await PutHabitAsync(
                client,
                habit.Id,
                updateRequest,
                csrfToken);

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
                        nameof(UpdateHabitRequest.Name),
                        StringComparison.OrdinalIgnoreCase));

        Assert.NotEmpty(nameError.Value);
    }

    [Fact]
    public async Task UpdateHabit_WhenDailyTargetCountIsNotOne_ReturnsBadRequestProblemDetails()
    {
        using var client = CreateClient();

        var registration =
            await RegisterAsync(client);

        var habit =
            await SeedHabitAsync(
                registration.User.Id,
                "Existing habit",
                DateTime.UtcNow.AddDays(-1),
                isActive: true);

        var csrfToken =
            await GetCsrfTokenAsync(client);

        var updateRequest =
            CreateValidUpdateHabitRequest();

        updateRequest.FrequencyType =
            HabitFrequencyType.Daily;

        updateRequest.TargetCount = 2;

        var response =
            await PutHabitAsync(
                client,
                habit.Id,
                updateRequest,
                csrfToken);

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
            $"/api/habits/{habit.Id}",
            problemDetails.Instance);
    }

    [Fact]
    public async Task UpdateHabit_WhenHabitBelongsToAnotherUser_ReturnsNotFoundAndDoesNotModifyHabit()
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
                DateTime.UtcNow.AddDays(-1),
                isActive: true);

        var csrfToken =
            await GetCsrfTokenAsync(
                requestingClient);

        var updateRequest =
            CreateValidUpdateHabitRequest();

        updateRequest.Name =
            "Attempted unauthorized update";

        var response =
            await PutHabitAsync(
                requestingClient,
                habit.Id,
                updateRequest,
                csrfToken);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        var ownerResponse =
            await ownerClient.GetAsync(
                $"/api/habits/{habit.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            ownerResponse.StatusCode);

        var unchangedHabit =
            await ownerResponse.Content
                .ReadFromJsonAsync<HabitResponse>(
                    _jsonOptions);

        Assert.NotNull(unchangedHabit);

        Assert.Equal(
            "Private habit",
            unchangedHabit.Name);
    }

    [Fact]
    public async Task UpdateHabit_WhenHabitDoesNotExist_ReturnsNotFound()
    {
        using var client = CreateClient();

        await RegisterAsync(client);

        var csrfToken =
            await GetCsrfTokenAsync(client);

        var missingHabitId =
            Guid.CreateVersion7();

        var response =
            await PutHabitAsync(
                client,
                missingHabitId,
                CreateValidUpdateHabitRequest(),
                csrfToken);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task DeactivateHabit_WhenAnonymous_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response =
            await DeleteHabitAsync(
                client,
                Guid.CreateVersion7());

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task DeactivateHabit_WithoutCsrfToken_ReturnsBadRequest()
    {
        using var client = CreateClient();

        var registration =
            await RegisterAsync(client);

        var habit =
            await SeedHabitAsync(
                registration.User.Id,
                "Active habit",
                DateTime.UtcNow.AddDays(-1),
                isActive: true);

        var response =
            await DeleteHabitAsync(
                client,
                habit.Id);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task DeactivateHabit_WhenOwnedHabitIsActive_DeactivatesAndUpdatesListVisibility()
    {
        using var client = CreateClient();

        var registration =
            await RegisterAsync(client);

        var createdAtUtc =
            DateTime.UtcNow.AddDays(-2);

        var habit =
            await SeedHabitAsync(
                registration.User.Id,
                "Active habit",
                createdAtUtc,
                isActive: true);

        var csrfToken =
            await GetCsrfTokenAsync(client);

        var response =
            await DeleteHabitAsync(
                client,
                habit.Id,
                csrfToken);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var responseBody =
            await response.Content
                .ReadFromJsonAsync<HabitResponse>(
                    _jsonOptions);

        Assert.NotNull(responseBody);

        Assert.Equal(
            habit.Id,
            responseBody.Id);

        Assert.False(
            responseBody.IsActive);

        Assert.Equal(
            createdAtUtc,
            responseBody.CreatedAtUtc);

        Assert.True(
            responseBody.UpdatedAtUtc
                > createdAtUtc);

        var activeListResponse =
            await client.GetAsync(
                "/api/habits");

        Assert.Equal(
            HttpStatusCode.OK,
            activeListResponse.StatusCode);

        var activeHabits =
            await activeListResponse.Content
                .ReadFromJsonAsync<List<HabitResponse>>(
                    _jsonOptions);

        Assert.NotNull(activeHabits);

        Assert.DoesNotContain(
            activeHabits,
            activeHabit =>
                activeHabit.Id == habit.Id);

        var completeListResponse =
            await client.GetAsync(
                "/api/habits?includeInactive=true");

        Assert.Equal(
            HttpStatusCode.OK,
            completeListResponse.StatusCode);

        var completeHabits =
            await completeListResponse.Content
                .ReadFromJsonAsync<List<HabitResponse>>(
                    _jsonOptions);

        Assert.NotNull(completeHabits);

        var inactiveHabit =
            Assert.Single(
                completeHabits,
                listedHabit =>
                    listedHabit.Id == habit.Id);

        Assert.False(
            inactiveHabit.IsActive);

        var getResponse =
            await client.GetAsync(
                $"/api/habits/{habit.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);
    }

    [Fact]
    public async Task DeactivateHabit_WhenCalledTwice_PreservesTimestampOnSecondRequest()
    {
        using var client = CreateClient();

        var registration =
            await RegisterAsync(client);

        var habit =
            await SeedHabitAsync(
                registration.User.Id,
                "Active habit",
                DateTime.UtcNow.AddDays(-1),
                isActive: true);

        var csrfToken =
            await GetCsrfTokenAsync(client);

        var firstResponse =
            await DeleteHabitAsync(
                client,
                habit.Id,
                csrfToken);

        Assert.Equal(
            HttpStatusCode.OK,
            firstResponse.StatusCode);

        var firstResponseBody =
            await firstResponse.Content
                .ReadFromJsonAsync<HabitResponse>(
                    _jsonOptions);

        Assert.NotNull(firstResponseBody);
        Assert.False(firstResponseBody.IsActive);

        var secondResponse =
            await DeleteHabitAsync(
                client,
                habit.Id,
                csrfToken);

        Assert.Equal(
            HttpStatusCode.OK,
            secondResponse.StatusCode);

        var secondResponseBody =
            await secondResponse.Content
                .ReadFromJsonAsync<HabitResponse>(
                    _jsonOptions);

        Assert.NotNull(secondResponseBody);
        Assert.False(secondResponseBody.IsActive);

        Assert.Equal(
            firstResponseBody.UpdatedAtUtc,
            secondResponseBody.UpdatedAtUtc);
    }

    [Fact]
    public async Task DeactivateHabit_WhenHabitBelongsToAnotherUser_ReturnsNotFoundAndDoesNotModifyHabit()
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
                DateTime.UtcNow.AddDays(-1),
                isActive: true);

        var csrfToken =
            await GetCsrfTokenAsync(
                requestingClient);

        var response =
            await DeleteHabitAsync(
                requestingClient,
                habit.Id,
                csrfToken);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        var ownerResponse =
            await ownerClient.GetAsync(
                $"/api/habits/{habit.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            ownerResponse.StatusCode);

        var unchangedHabit =
            await ownerResponse.Content
                .ReadFromJsonAsync<HabitResponse>(
                    _jsonOptions);

        Assert.NotNull(unchangedHabit);
        Assert.True(unchangedHabit.IsActive);
    }

    [Fact]
    public async Task DeactivateHabit_WhenHabitDoesNotExist_ReturnsNotFound()
    {
        using var client = CreateClient();

        await RegisterAsync(client);

        var csrfToken =
            await GetCsrfTokenAsync(client);

        var response =
            await DeleteHabitAsync(
                client,
                Guid.CreateVersion7(),
                csrfToken);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    private static async Task<HttpResponseMessage> DeleteHabitAsync(
    HttpClient client,
    Guid habitId,
    string? csrfToken = null)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Delete,
                $"/api/habits/{habitId}");

        if (!string.IsNullOrWhiteSpace(
            csrfToken))
        {
            request.Headers.Add(
                "X-CSRF-TOKEN",
                csrfToken);
        }

        return await client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> PutHabitAsync(
    HttpClient client,
    Guid habitId,
    UpdateHabitRequest updateRequest,
    string? csrfToken = null)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Put,
                $"/api/habits/{habitId}")
            {
                Content =
                    JsonContent.Create(
                        updateRequest,
                        options: _jsonOptions)
            };

        if (!string.IsNullOrWhiteSpace(
            csrfToken))
        {
            request.Headers.Add(
                "X-CSRF-TOKEN",
                csrfToken);
        }

        return await client.SendAsync(request);
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

    private static UpdateHabitRequest CreateValidUpdateHabitRequest()
    {
        return new UpdateHabitRequest
        {
            Name = "Updated habit",
            Description = "Updated description",
            Category = "Learning",
            FrequencyType =
                HabitFrequencyType.Weekly,
            TargetCount = 3,
            Difficulty =
                HabitDifficulty.Hard
        };
    }
}