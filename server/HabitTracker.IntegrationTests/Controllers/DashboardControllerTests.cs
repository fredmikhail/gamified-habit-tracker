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

public sealed class DashboardControllerTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public DashboardControllerTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;

        _jsonOptions =
            factory.Services
                .GetRequiredService<
                    IOptions<JsonOptions>>()
                .Value
                .JsonSerializerOptions;
    }

    [Fact]
    public async Task GetDashboard_WhenAnonymous_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response =
            await client.GetAsync(
                "/api/dashboard");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetDashboard_WhenAuthenticatedWithoutXp_ReturnsInitialProgress()
    {
        using var client = CreateClient();

        await RegisterAsync(client);

        var response =
            await client.GetAsync(
                "/api/dashboard");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var dashboard =
            await response.Content
                .ReadFromJsonAsync<DashboardResponse>(
                    _jsonOptions);

        Assert.NotNull(dashboard);

        Assert.Equal(
            0,
            dashboard.OverallProgress.TotalXp);

        Assert.Equal(
            1,
            dashboard.OverallProgress.Level);

        Assert.Equal(
            0,
            dashboard.OverallProgress
                .XpIntoCurrentLevel);

        Assert.Equal(
            200,
            dashboard.OverallProgress
                .XpNeededForNextLevel);
    }

    [Fact]
    public async Task GetDashboard_WhenAuthenticated_ReturnsOnlyOwnedXpProgress()
    {
        using var client = CreateClient();
        using var otherClient = CreateClient();

        var registration =
            await RegisterAsync(client);

        var otherRegistration =
            await RegisterAsync(otherClient);

        await SeedXpTransactionsAsync(
            registration.User.Id,
            otherRegistration.User.Id);

        var response =
            await client.GetAsync(
                "/api/dashboard");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var dashboard =
            await response.Content
                .ReadFromJsonAsync<DashboardResponse>(
                    _jsonOptions);

        Assert.NotNull(dashboard);

        Assert.Equal(
            300,
            dashboard.OverallProgress.TotalXp);

        Assert.Equal(
            2,
            dashboard.OverallProgress.Level);

        Assert.Equal(
            100,
            dashboard.OverallProgress
                .XpIntoCurrentLevel);

        Assert.Equal(
            250,
            dashboard.OverallProgress
                .XpNeededForNextLevel);
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

    private async Task SeedXpTransactionsAsync(
        Guid userId,
        Guid otherUserId)
    {
        using var scope =
            _factory.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        var createdAtUtc = DateTime.UtcNow;

        dbContext.XpTransactions.AddRange(
            CreateTransaction(
                userId,
                AttributeType.Fitness,
                140,
                createdAtUtc),
            CreateTransaction(
                userId,
                AttributeType.Discipline,
                100,
                createdAtUtc.AddMinutes(1)),
            CreateTransaction(
                userId,
                AttributeType.Focus,
                60,
                createdAtUtc.AddMinutes(2)),
            CreateTransaction(
                otherUserId,
                AttributeType.Mind,
                999,
                createdAtUtc.AddMinutes(3)));

        await dbContext.SaveChangesAsync();
    }

    private static XpTransaction CreateTransaction(
        Guid userId,
        AttributeType attributeType,
        int amount,
        DateTime createdAtUtc)
    {
        return new XpTransaction
        {
            UserId = userId,
            HabitCompletionId =
                Guid.CreateVersion7(),
            AttributeType = attributeType,
            Amount = amount,
            Reason = "Dashboard integration test",
            CreatedAtUtc = createdAtUtc
        };
    }

    private static async Task<AuthResponse> RegisterAsync(
        HttpClient client)
    {
        var csrfToken =
            await GetCsrfTokenAsync(client);

        var uniqueSuffix =
            Guid.NewGuid().ToString("N");

        var requestBody =
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
                    JsonContent.Create(requestBody)
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

    private static async Task<string> GetCsrfTokenAsync(
        HttpClient client)
    {
        var response =
            await client.GetAsync(
                "/api/auth/csrf-token");

        response.EnsureSuccessStatusCode();

        var responseBody =
            await response.Content
                .ReadFromJsonAsync<
                    AntiforgeryTokenResponse>();

        Assert.NotNull(responseBody);

        return responseBody.RequestToken;
    }
}
