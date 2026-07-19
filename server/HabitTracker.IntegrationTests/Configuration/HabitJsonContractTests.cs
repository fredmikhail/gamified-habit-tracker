using System.Text.Json;
using HabitTracker.Api.Domain.Enums;
using HabitTracker.Api.DTOs;
using HabitTracker.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HabitTracker.IntegrationTests.Configuration;

public sealed class HabitJsonContractTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly JsonSerializerOptions _jsonOptions;

    public HabitJsonContractTests(
        CustomWebApplicationFactory factory)
    {
        _jsonOptions =
            factory.Services
                .GetRequiredService<IOptions<JsonOptions>>()
                .Value
                .JsonSerializerOptions;
    }

    [Fact]
    public void HabitResponse_WhenSerialized_UsesCamelCaseStringEnums()
    {
        var timestampUtc = DateTime.UtcNow;

        var response = new HabitResponse
        {
            Id = Guid.CreateVersion7(),
            Name = "Go to gym",
            Description = "Complete a planned gym session.",
            Category = "Fitness",
            FrequencyType = HabitFrequencyType.Weekly,
            TargetCount = 3,
            Difficulty = HabitDifficulty.Elite,
            IsActive = true,
            CreatedAtUtc = timestampUtc,
            UpdatedAtUtc = timestampUtc
        };

        var json =
            JsonSerializer.Serialize(
                response,
                _jsonOptions);

        using var document =
            JsonDocument.Parse(json);

        var root = document.RootElement;

        Assert.Equal(
            "weekly",
            root.GetProperty("frequencyType").GetString());

        Assert.Equal(
            "elite",
            root.GetProperty("difficulty").GetString());
    }

    [Fact]
    public void CreateHabitRequest_WhenStringEnumsAreDeserialized_ReturnsExpectedValues()
    {
        const string json =
            """
            {
              "name": "Go to gym",
              "description": null,
              "category": "Fitness",
              "frequencyType": "weekly",
              "targetCount": 3,
              "difficulty": "elite"
            }
            """;

        var request =
            JsonSerializer.Deserialize<CreateHabitRequest>(
                json,
                _jsonOptions);

        Assert.NotNull(request);

        Assert.Equal(
            HabitFrequencyType.Weekly,
            request.FrequencyType);

        Assert.Equal(
            HabitDifficulty.Elite,
            request.Difficulty);

        Assert.Equal(3, request.TargetCount);
    }

    [Fact]
    public void CreateHabitRequest_WhenNumericEnumsAreDeserialized_ThrowsJsonException()
    {
        const string json =
            """
            {
              "name": "Go to gym",
              "frequencyType": 2,
              "targetCount": 3,
              "difficulty": 4
            }
            """;

        Assert.Throws<JsonException>(
            () =>
                JsonSerializer.Deserialize<CreateHabitRequest>(
                    json,
                    _jsonOptions));
    }
}