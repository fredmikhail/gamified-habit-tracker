using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HabitTracker.Api.Data;
using HabitTracker.Api.Domain.Entities;
using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.DTOs;
using HabitTracker.Api.Services;
using HabitTracker.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HabitTracker.IntegrationTests.Controllers;

public sealed class CompletionsControllerTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public CompletionsControllerTests(
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
    public async Task CompleteHabit_WhenAnonymous_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response =
            await client.PostAsJsonAsync(
                $"/api/habits/{Guid.CreateVersion7()}/completions",
                new CompleteHabitRequest(),
                _jsonOptions);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task CompleteHabit_WithoutCsrfToken_ReturnsBadRequest()
    {
        using var client = CreateClient();

        var registration =
            await RegisterAsync(client);

        var habit =
            await SeedHabitAsync(
                registration.User.Id,
                isActive: true);

        var response =
            await client.PostAsJsonAsync(
                $"/api/habits/{habit.Id}/completions",
                new CompleteHabitRequest(),
                _jsonOptions);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task CompleteHabit_WhenHabitDoesNotExist_ReturnsNotFound()
    {
        using var client = CreateClient();

        await RegisterAsync(client);

        var csrfToken =
            await GetCsrfTokenAsync(client);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/habits/{Guid.CreateVersion7()}/completions")
            {
                Content =
                    JsonContent.Create(
                        new CompleteHabitRequest(),
                        options: _jsonOptions)
            };

        request.Headers.Add(
            "X-CSRF-TOKEN",
            csrfToken);

        var response =
            await client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task CompleteHabit_WhenHabitBelongsToAnotherUser_ReturnsNotFoundAndDoesNotCreateCompletion()
    {
        using var ownerClient = CreateClient();
        using var requestingClient = CreateClient();

        var ownerRegistration =
            await RegisterAsync(ownerClient);

        await RegisterAsync(requestingClient);

        var habit =
            await SeedHabitAsync(
                ownerRegistration.User.Id,
                isActive: true);

        var csrfToken =
            await GetCsrfTokenAsync(
                requestingClient);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/habits/{habit.Id}/completions")
            {
                Content =
                    JsonContent.Create(
                        new CompleteHabitRequest(),
                        options: _jsonOptions)
            };

        request.Headers.Add(
            "X-CSRF-TOKEN",
            csrfToken);

        var response =
            await requestingClient.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        using var scope =
            _factory.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        Assert.DoesNotContain(
            dbContext.HabitCompletions,
            completion =>
                completion.HabitId == habit.Id);
    }

    [Fact]
    public async Task CompleteHabit_WhenHabitIsInactive_ReturnsConflictProblemDetails()
    {
        using var client = CreateClient();

        var registration =
            await RegisterAsync(client);

        var habit =
            await SeedHabitAsync(
                registration.User.Id,
                isActive: false);

        var csrfToken =
            await GetCsrfTokenAsync(client);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/habits/{habit.Id}/completions")
            {
                Content =
                    JsonContent.Create(
                        new CompleteHabitRequest(),
                        options: _jsonOptions)
            };

        request.Headers.Add(
            "X-CSRF-TOKEN",
            csrfToken);

        var response =
            await client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);

        var problemDetails =
            await response.Content
                .ReadFromJsonAsync<ProblemDetails>(
                    _jsonOptions);

        Assert.NotNull(problemDetails);

        Assert.Equal(
            StatusCodes.Status409Conflict,
            problemDetails.Status);

        Assert.Equal(
            "Inactive habit",
            problemDetails.Title);

        Assert.Equal(
            "Inactive habits cannot be completed.",
            problemDetails.Detail);

        using var scope =
            _factory.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        Assert.DoesNotContain(
            dbContext.HabitCompletions,
            completion =>
                completion.HabitId == habit.Id);
    }

    [Fact]
    public async Task CompleteHabit_WhenAlreadyCompletedToday_ReturnsConflictAndKeepsSingleCompletion()
    {
        using var client = CreateClient();

        var registration =
            await RegisterAsync(client);

        var habit =
            await SeedHabitAsync(
                registration.User.Id,
                isActive: true);

        var csrfToken =
            await GetCsrfTokenAsync(client);

        using var firstRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/habits/{habit.Id}/completions")
            {
                Content =
                    JsonContent.Create(
                        new CompleteHabitRequest(),
                        options: _jsonOptions)
            };

        firstRequest.Headers.Add(
            "X-CSRF-TOKEN",
            csrfToken);

        var firstResponse =
            await client.SendAsync(firstRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            firstResponse.StatusCode);

        using var secondRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/habits/{habit.Id}/completions")
            {
                Content =
                    JsonContent.Create(
                        new CompleteHabitRequest(),
                        options: _jsonOptions)
            };

        secondRequest.Headers.Add(
            "X-CSRF-TOKEN",
            csrfToken);

        var secondResponse =
            await client.SendAsync(secondRequest);

        Assert.Equal(
            HttpStatusCode.Conflict,
            secondResponse.StatusCode);

        var problemDetails =
            await secondResponse.Content
                .ReadFromJsonAsync<ProblemDetails>(
                    _jsonOptions);

        Assert.NotNull(problemDetails);

        Assert.Equal(
            StatusCodes.Status409Conflict,
            problemDetails.Status);

        Assert.Equal(
            "Habit already completed",
            problemDetails.Title);

        Assert.Equal(
            "This habit has already been completed for today.",
            problemDetails.Detail);

        using var scope =
            _factory.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        Assert.Single(
            dbContext.HabitCompletions,
            completion =>
                completion.HabitId == habit.Id);
    }

    [Fact]
    public async Task CompleteHabit_WhenNotesExceedMaximumLength_ReturnsBadRequestAndDoesNotCreateCompletion()
    {
        using var client = CreateClient();

        var registration =
            await RegisterAsync(client);

        var habit =
            await SeedHabitAsync(
                registration.User.Id,
                isActive: true);

        var csrfToken =
            await GetCsrfTokenAsync(client);

        var completeRequest =
            new CompleteHabitRequest
            {
                Notes = new string('a', 501)
            };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/habits/{habit.Id}/completions")
            {
                Content =
                    JsonContent.Create(
                        completeRequest,
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
                .ReadFromJsonAsync<ValidationProblemDetails>(
                    _jsonOptions);

        Assert.NotNull(problemDetails);

        Assert.Equal(
            StatusCodes.Status400BadRequest,
            problemDetails.Status);

        var notesError =
            Assert.Single(
                problemDetails.Errors,
                error =>
                    string.Equals(
                        error.Key,
                        nameof(CompleteHabitRequest.Notes),
                        StringComparison.OrdinalIgnoreCase));

        Assert.NotEmpty(notesError.Value);

        using var scope =
            _factory.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        Assert.DoesNotContain(
            dbContext.HabitCompletions,
            completion =>
                completion.HabitId == habit.Id);
    }

    [Fact]
    public async Task CompleteHabit_WithValidRequest_ReturnsCreatedCompletion()
    {
        using var client = CreateClient();

        var registration =
            await RegisterAsync(client);

        var habit =
            await SeedHabitAsync(
                registration.User.Id,
                isActive: true);

        var csrfToken =
            await GetCsrfTokenAsync(client);

        var completeRequest =
            new CompleteHabitRequest
            {
                Notes = "  Strong workout today.  "
            };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/habits/{habit.Id}/completions")
            {
                Content =
                    JsonContent.Create(
                        completeRequest,
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
                .ReadFromJsonAsync<CompleteHabitResponse>(
                    _jsonOptions);

        Assert.NotNull(responseBody);

        Assert.NotEqual(
            Guid.Empty,
            responseBody.Completion.Id);

        Assert.Equal(
            habit.Id,
            responseBody.Completion.HabitId);

        Assert.Equal(
            "Strong workout today.",
            responseBody.Completion.Notes);

        Assert.NotEqual(
            default,
            responseBody.Completion.CompletedAtUtc);

        var expectedCompletedDate =
            LocalDateCalculator.GetLocalDate(
                new DateTimeOffset(
                    responseBody.Completion.CompletedAtUtc),
                registration.User.TimeZone);

        Assert.Equal(
            expectedCompletedDate,
            responseBody.Completion.CompletedDate);

        using var scope =
            _factory.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        var savedCompletion =
            Assert.Single(
                dbContext.HabitCompletions,
                completion =>
                    completion.Id
                        == responseBody.Completion.Id);

        Assert.Equal(
            registration.User.Id,
            savedCompletion.UserId);

        Assert.Equal(
            habit.Id,
            savedCompletion.HabitId);

        Assert.Equal(
            responseBody.Completion.CompletedDate,
            savedCompletion.CompletedDate);

        Assert.Equal(
            responseBody.Completion.CompletedAtUtc,
            savedCompletion.CompletedAtUtc);

        Assert.Equal(
            "Strong workout today.",
            savedCompletion.Notes);
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
                    $"completion_{uniqueSuffix}@example.com",
                Username =
                    $"complete_{uniqueSuffix[..8]}",
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
        bool isActive)
    {
        using var scope =
            _factory.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        var timestampUtc =
            DateTime.UtcNow.AddDays(-1);

        var habit = new Habit
        {
            UserId = userId,
            Name = "Go to gym",
            Category = "Fitness",
            FrequencyType =
                HabitFrequencyType.Daily,
            TargetCount = 1,
            Difficulty =
                HabitDifficulty.Medium,
            IsActive = isActive,
            CreatedAtUtc = timestampUtc,
            UpdatedAtUtc = timestampUtc
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
}
