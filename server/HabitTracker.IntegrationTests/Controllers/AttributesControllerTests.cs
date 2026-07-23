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

    [Fact]
    public async Task GetAttributeOverview_WhenAnonymous_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response =
            await client.GetAsync(
                "/api/attributes/overview");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetAttributeOverview_WhenAuthenticated_ReturnsOwnedCharacterProgression()
    {
        using var client = CreateClient();
        using var otherClient = CreateClient();

        var registration =
            await RegisterAsync(client);

        var otherRegistration =
            await RegisterAsync(otherClient);

        await SeedAttributeOverviewAsync(
            registration.User.Id,
            otherRegistration.User.Id);

        var response =
            await client.GetAsync(
                "/api/attributes/overview");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var overview =
            await response.Content
                .ReadFromJsonAsync<
                    AttributeOverviewResponse>(
                        _jsonOptions);

        Assert.NotNull(overview);

        Assert.Equal(
            8,
            overview.Attributes.Count);

        Assert.Equal(
            10060,
            overview.TotalAttributeXp);

        Assert.Equal(
            68,
            overview.BalanceScore);

        Assert.NotNull(
            overview.StrongestAttribute);

        Assert.Equal(
            AttributeType.Discipline,
            overview.StrongestAttribute
                .AttributeType);

        Assert.NotNull(
            overview.NeedsFocusAttribute);

        Assert.Equal(
            AttributeType.Social,
            overview.NeedsFocusAttribute
                .AttributeType);

        Assert.Collection(
            overview.ClosestToLevelUp,
            first =>
            {
                Assert.Equal(
                    AttributeType.Purpose,
                    first.AttributeType);

                Assert.Equal(
                    80,
                    first.XpRemaining);
            },
            second =>
            {
                Assert.Equal(
                    AttributeType.Vitality,
                    second.AttributeType);

                Assert.Equal(
                    85,
                    second.XpRemaining);
            },
            third =>
            {
                Assert.Equal(
                    AttributeType.Mind,
                    third.AttributeType);

                Assert.Equal(
                    90,
                    third.XpRemaining);
            });

        Assert.Collection(
            overview.RecentXpTransactions,
            newest =>
            {
                Assert.Equal(
                    "Meditate",
                    newest.HabitName);

                Assert.Equal(
                    AttributeType.Focus,
                    newest.AttributeType);

                Assert.Equal(
                    -25,
                    newest.Amount);

                Assert.Equal(
                    "Habit completion undo",
                    newest.Reason);
            },
            oldest =>
            {
                Assert.Equal(
                    "Read C# textbook",
                    oldest.HabitName);

                Assert.Equal(
                    AttributeType.Mind,
                    oldest.AttributeType);

                Assert.Equal(
                    30,
                    oldest.Amount);

                Assert.Equal(
                    "Habit completion",
                    oldest.Reason);
            });
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

    private async Task SeedAttributeOverviewAsync(
        Guid userId,
        Guid otherUserId)
    {
        using var scope =
            _factory.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        var currentXpByAttribute =
            new Dictionary<AttributeType, int>
            {
                [AttributeType.Discipline] = 1860,
                [AttributeType.Fitness] = 1300,
                [AttributeType.Vitality] = 890,
                [AttributeType.Focus] = 1250,
                [AttributeType.Mind] = 1710,
                [AttributeType.Resilience] = 980,
                [AttributeType.Social] = 650,
                [AttributeType.Purpose] = 1420
            };

        dbContext.UserAttributes.AddRange(
            currentXpByAttribute.Select(
                item =>
                    new UserAttribute
                    {
                        UserId = userId,
                        AttributeType = item.Key,
                        CurrentXp = item.Value,
                        UpdatedAtUtc =
                            DateTime.UtcNow
                    }));

        var baseTimestampUtc =
            DateTime.UtcNow.AddMinutes(-3);

        AddXpHistory(
            dbContext,
            userId,
            "Read C# textbook",
            AttributeType.Mind,
            30,
            "Habit completion",
            baseTimestampUtc);

        AddXpHistory(
            dbContext,
            userId,
            "Meditate",
            AttributeType.Focus,
            -25,
            "Habit completion undo",
            baseTimestampUtc.AddMinutes(1));

        AddXpHistory(
            dbContext,
            otherUserId,
            "Private habit",
            AttributeType.Fitness,
            999,
            "Habit completion",
            baseTimestampUtc.AddMinutes(2));

        await dbContext.SaveChangesAsync();
    }

    private static void AddXpHistory(
        AppDbContext dbContext,
        Guid userId,
        string habitName,
        AttributeType attributeType,
        int amount,
        string reason,
        DateTime createdAtUtc)
    {
        var habit =
            new Habit
            {
                UserId = userId,
                Name = habitName,
                Category =
                    HabitCategory.GeneralGrowth,
                FrequencyType =
                    HabitFrequencyType.Daily,
                TargetCount = 1,
                Difficulty =
                    HabitDifficulty.Medium,
                IsActive = true,
                CreatedAtUtc =
                    createdAtUtc.AddDays(-1),
                UpdatedAtUtc =
                    createdAtUtc.AddDays(-1)
            };

        var configuration =
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
                    DateOnly.FromDateTime(
                        createdAtUtc.AddDays(-1)),
                CreatedAtUtc =
                    createdAtUtc.AddDays(-1)
            };

        habit.HabitConfigurationVersions.Add(
            configuration);

        var completion =
            new HabitCompletion
            {
                UserId = userId,
                HabitId = habit.Id,
                HabitConfigurationVersionId =
                    configuration.Id,
                CompletedDate =
                    DateOnly.FromDateTime(
                        createdAtUtc),
                CompletedAtUtc =
                    createdAtUtc
            };

        habit.HabitCompletions.Add(
            completion);

        completion.XpTransactions.Add(
            new XpTransaction
            {
                UserId = userId,
                HabitCompletionId =
                    completion.Id,
                AttributeType =
                    attributeType,
                Amount = amount,
                Reason = reason,
                CreatedAtUtc =
                    createdAtUtc
            });

        dbContext.Habits.Add(
            habit);
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
