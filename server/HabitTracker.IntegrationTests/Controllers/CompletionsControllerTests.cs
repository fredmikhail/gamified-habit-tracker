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
using Microsoft.EntityFrameworkCore;
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

        Assert.Collection(
    responseBody.Rewards,
    primaryReward =>
    {
        Assert.Equal(
            AttributeType.Fitness,
            primaryReward.AttributeType);

        Assert.Equal(
            14,
            primaryReward.XpAmount);
    },
    secondaryReward =>
    {
        Assert.Equal(
            AttributeType.Discipline,
            secondaryReward.AttributeType);

        Assert.Equal(
            6,
            secondaryReward.XpAmount);
    });

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

        var savedConfiguration =
 await dbContext
     .HabitConfigurationVersions
     .SingleAsync(configuration =>
         configuration.HabitId
             == habit.Id);

        Assert.Equal(
            savedConfiguration.Id,
            savedCompletion
                .HabitConfigurationVersionId);
    }

    [Fact]
    public async Task UndoToday_WhenCompletionExists_ReturnsNoContentAndRemovesCompletion()
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

        using var completeRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/habits/{habit.Id}/completions")
            {
                Content =
                    JsonContent.Create(
                        new CompleteHabitRequest
                        {
                            Notes = "Completed before undo."
                        },
                        options: _jsonOptions)
            };

        completeRequest.Headers.Add(
            "X-CSRF-TOKEN",
            csrfToken);

        var completeResponse =
            await client.SendAsync(
                completeRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            completeResponse.StatusCode);

        using var undoRequest =
            new HttpRequestMessage(
                HttpMethod.Delete,
                $"/api/habits/{habit.Id}/completions/today");

        undoRequest.Headers.Add(
            "X-CSRF-TOKEN",
            csrfToken);

        var undoResponse =
            await client.SendAsync(
                undoRequest);

        Assert.Equal(
            HttpStatusCode.NoContent,
            undoResponse.StatusCode);

        Assert.Equal(
            string.Empty,
            await undoResponse.Content.ReadAsStringAsync());

        using var scope =
            _factory.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        Assert.DoesNotContain(
            dbContext.HabitCompletions,
            completion =>
                completion.HabitId == habit.Id);

        Assert.Contains(
            dbContext.Habits,
            savedHabit =>
                savedHabit.Id == habit.Id);
    }

    [Fact]
    public async Task UndoToday_WhenAnonymous_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        using var request =
            new HttpRequestMessage(
                HttpMethod.Delete,
                $"/api/habits/{Guid.CreateVersion7()}/completions/today");

        var response =
            await client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task UndoToday_WithoutCsrfToken_ReturnsBadRequest()
    {
        using var client = CreateClient();

        var registration =
            await RegisterAsync(client);

        var habit =
            await SeedHabitAsync(
                registration.User.Id,
                isActive: true);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Delete,
                $"/api/habits/{habit.Id}/completions/today");

        var response =
            await client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task UndoToday_WhenHabitDoesNotExist_ReturnsNotFound()
    {
        using var client = CreateClient();

        await RegisterAsync(client);

        var csrfToken =
            await GetCsrfTokenAsync(client);

        using var response =
            await DeleteTodayCompletionAsync(
                client,
                Guid.CreateVersion7(),
                csrfToken);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task UndoToday_WhenHabitBelongsToAnotherUser_ReturnsNotFoundAndKeepsCompletion()
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

        var ownerCsrfToken =
            await GetCsrfTokenAsync(ownerClient);

        using var completeRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/habits/{habit.Id}/completions")
            {
                Content =
                    JsonContent.Create(
                        new CompleteHabitRequest(),
                        options: _jsonOptions)
            };

        completeRequest.Headers.Add(
            "X-CSRF-TOKEN",
            ownerCsrfToken);

        using var completeResponse =
            await ownerClient.SendAsync(
                completeRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            completeResponse.StatusCode);

        var requestingCsrfToken =
            await GetCsrfTokenAsync(
                requestingClient);

        using var undoResponse =
            await DeleteTodayCompletionAsync(
                requestingClient,
                habit.Id,
                requestingCsrfToken);

        Assert.Equal(
            HttpStatusCode.NotFound,
            undoResponse.StatusCode);

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
    public async Task UndoToday_WhenNoCompletionExistsToday_ReturnsNotFound()
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

        using var response =
            await DeleteTodayCompletionAsync(
                client,
                habit.Id,
                csrfToken);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task UndoToday_WhenHabitIsInactive_ReturnsNoContentAndRemovesCompletion()
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

        using var completeRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/habits/{habit.Id}/completions")
            {
                Content =
                    JsonContent.Create(
                        new CompleteHabitRequest(),
                        options: _jsonOptions)
            };

        completeRequest.Headers.Add(
            "X-CSRF-TOKEN",
            csrfToken);

        using var completeResponse =
            await client.SendAsync(
                completeRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            completeResponse.StatusCode);

        using (var scope =
            _factory.Services.CreateScope())
        {
            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

            var savedHabit =
                Assert.Single(
                    dbContext.Habits,
                    candidate =>
                        candidate.Id == habit.Id);

            savedHabit.IsActive = false;
            savedHabit.UpdatedAtUtc =
                DateTime.UtcNow;

            await dbContext.SaveChangesAsync();
        }

        using var undoResponse =
            await DeleteTodayCompletionAsync(
                client,
                habit.Id,
                csrfToken);

        Assert.Equal(
            HttpStatusCode.NoContent,
            undoResponse.StatusCode);

        using var verificationScope =
            _factory.Services.CreateScope();

        var verificationDbContext =
            verificationScope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        Assert.DoesNotContain(
            verificationDbContext.HabitCompletions,
            completion =>
                completion.HabitId == habit.Id);

        var inactiveHabit =
            Assert.Single(
                verificationDbContext.Habits,
                candidate =>
                    candidate.Id == habit.Id);

        Assert.False(inactiveHabit.IsActive);
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

        var timeZoneId =
            await dbContext.UserSettings
                .Where(settings =>
                    settings.UserId == userId)
                .Select(settings =>
                    settings.TimeZone)
                .SingleAsync();

        var timestampUtc =
            DateTime.UtcNow.AddDays(-1);

        var timestampOffset =
            new DateTimeOffset(
                DateTime.SpecifyKind(
                    timestampUtc,
                    DateTimeKind.Utc));

        var effectiveFromDate =
            LocalDateCalculator.GetLocalDate(
                timestampOffset,
                timeZoneId);

        var habit = new Habit
        {
            UserId = userId,
            Name = "Go to gym",
            Category =
                HabitCategory.FitnessAndMovement,
            FrequencyType =
                HabitFrequencyType.Daily,
            TargetCount = 1,
            Difficulty =
                HabitDifficulty.Medium,
            IsActive = isActive,
            CreatedAtUtc = timestampUtc,
            UpdatedAtUtc = timestampUtc
        };

        habit.HabitConfigurationVersions.Add(
            new HabitConfigurationVersion
            {
                HabitId = habit.Id,
                VersionNumber = 1,
                Category = habit.Category,
                FrequencyType =
                    habit.FrequencyType,
                TargetCount =
                    habit.TargetCount,
                Difficulty =
                    habit.Difficulty,
                EffectiveFromDate =
                    effectiveFromDate,
                CreatedAtUtc =
                    timestampUtc
            });

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

    private static async Task<HttpResponseMessage> DeleteTodayCompletionAsync(
    HttpClient client,
    Guid habitId,
    string? csrfToken = null)
    {
        using var request =
            new HttpRequestMessage(
                HttpMethod.Delete,
                $"/api/habits/{habitId}/completions/today");

        if (!string.IsNullOrWhiteSpace(csrfToken))
        {
            request.Headers.Add(
                "X-CSRF-TOKEN",
                csrfToken);
        }

        return await client.SendAsync(request);
    }
}
