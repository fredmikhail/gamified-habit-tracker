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

public sealed class AttributesControllerTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public AttributesControllerTests(
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
    public async Task GetAttributes_WhenAnonymous_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response =
            await client.GetAsync(
                "/api/attributes");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetAttributes_WhenAuthenticated_ReturnsOwnedProgressAndMissingAttributes()
    {
        using var client = CreateClient();
        using var otherClient = CreateClient();

        var registration =
            await RegisterAsync(client);

        var otherRegistration =
            await RegisterAsync(otherClient);

        await SeedUserAttributeAsync(
            registration.User.Id,
            AttributeType.Fitness,
            225);

        await SeedUserAttributeAsync(
            otherRegistration.User.Id,
            AttributeType.Fitness,
            999);

        var response =
            await client.GetAsync(
                "/api/attributes");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var attributes =
            await response.Content
                .ReadFromJsonAsync<
                    List<UserAttributeResponse>>(
                        _jsonOptions);

        Assert.NotNull(attributes);

        Assert.Equal(
            Enum.GetValues<AttributeType>(),
            attributes
                .Select(attribute =>
                    attribute.AttributeType)
                .ToArray());

        var fitness =
            Assert.Single(
                attributes,
                attribute =>
                    attribute.AttributeType
                    == AttributeType.Fitness);

        Assert.Equal(225, fitness.CurrentXp);
        Assert.Equal(3, fitness.Level);
        Assert.Equal(
            0,
            fitness.XpIntoCurrentLevel);
        Assert.Equal(
            150,
            fitness.XpNeededForNextLevel);

        var focus =
            Assert.Single(
                attributes,
                attribute =>
                    attribute.AttributeType
                    == AttributeType.Focus);

        Assert.Equal(0, focus.CurrentXp);
        Assert.Equal(1, focus.Level);
        Assert.Equal(
            0,
            focus.XpIntoCurrentLevel);
        Assert.Equal(
            100,
            focus.XpNeededForNextLevel);
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

    private async Task SeedUserAttributeAsync(
        Guid userId,
        AttributeType attributeType,
        int currentXp)
    {
        using var scope =
            _factory.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        dbContext.UserAttributes.Add(
            new UserAttribute
            {
                UserId = userId,
                AttributeType = attributeType,
                CurrentXp = currentXp,
                UpdatedAtUtc = DateTime.UtcNow
            });

        await dbContext.SaveChangesAsync();
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
