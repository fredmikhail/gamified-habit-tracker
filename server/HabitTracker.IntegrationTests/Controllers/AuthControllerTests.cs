using System.Net;
using System.Net.Http.Json;
using HabitTracker.Api.DTOs;
using HabitTracker.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HabitTracker.IntegrationTests.Controllers;

public sealed class AuthControllerTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
    }

    [Fact]
    public async Task GetCsrfToken_ReturnsTokenAndSetsAntiforgeryCookie()
    {
        var response =
            await _client.GetAsync("/api/auth/csrf-token");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var responseBody =
            await response.Content
                .ReadFromJsonAsync<AntiforgeryTokenResponse>();

        Assert.NotNull(responseBody);

        Assert.False(
            string.IsNullOrWhiteSpace(
                responseBody.RequestToken));

        Assert.True(
            response.Headers.TryGetValues(
                "Set-Cookie",
                out var setCookieHeaders));

        var antiforgeryCookie =
            Assert.Single(
                setCookieHeaders,
                header =>
                    header.StartsWith(
                        "HabitTracker.Antiforgery=",
                        StringComparison.Ordinal));

        Assert.Contains(
            "HttpOnly",
            antiforgeryCookie,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "SameSite=Lax",
            antiforgeryCookie,
            StringComparison.OrdinalIgnoreCase);
    }
}
