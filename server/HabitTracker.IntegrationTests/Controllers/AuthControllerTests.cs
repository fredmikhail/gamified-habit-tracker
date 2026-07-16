using System.Net;
using System.Net.Http.Json;
using HabitTracker.Api.DTOs;
using HabitTracker.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc;

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
                AllowAutoRedirect = false,
                HandleCookies = true
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

    [Fact]
    public async Task Register_WithValidRequest_ReturnsCreatedAndSetsSessionCookie()
    {
        var csrfResponse =
            await _client.GetAsync("/api/auth/csrf-token");

        csrfResponse.EnsureSuccessStatusCode();

        var csrfResponseBody =
            await csrfResponse.Content
                .ReadFromJsonAsync<AntiforgeryTokenResponse>();

        Assert.NotNull(csrfResponseBody);

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
            csrfResponseBody.RequestToken);

        var response =
            await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var responseBody =
            await response.Content
                .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(responseBody);

        Assert.NotEqual(
            Guid.Empty,
            responseBody.User.Id);

        Assert.Equal(
            registerRequest.Email,
            responseBody.User.Email);

        Assert.Equal(
            registerRequest.Username,
            responseBody.User.Username);

        Assert.Equal(
            registerRequest.Username,
            responseBody.User.DisplayName);

        Assert.Equal(
            registerRequest.TimeZone,
            responseBody.User.TimeZone);

        Assert.True(
            response.Headers.TryGetValues(
                "Set-Cookie",
                out var setCookieHeaders));

        var authenticationCookie =
            Assert.Single(
                setCookieHeaders,
                header =>
                    header.StartsWith(
                        "HabitTracker.Auth=",
                        StringComparison.Ordinal));

        Assert.Contains(
            "HttpOnly",
            authenticationCookie,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "SameSite=Lax",
            authenticationCookie,
            StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain(
            "Expires=",
            authenticationCookie,
            StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain(
            "Max-Age=",
            authenticationCookie,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_WithInvalidTimeZone_ReturnsBadRequestProblemDetails()
    {
        var csrfResponse =
            await _client.GetAsync("/api/auth/csrf-token");

        csrfResponse.EnsureSuccessStatusCode();

        var csrfResponseBody =
            await csrfResponse.Content
                .ReadFromJsonAsync<AntiforgeryTokenResponse>();

        Assert.NotNull(csrfResponseBody);

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
                    "Not/ARealTimeZone"
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
            csrfResponseBody.RequestToken);

        var response =
            await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problemDetails =
            await response.Content
                .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problemDetails);

        Assert.Equal(
            StatusCodes.Status400BadRequest,
            problemDetails.Status);

        Assert.Equal(
            "Invalid time zone",
            problemDetails.Title);

        Assert.Equal(
            "The supplied time zone is not a valid IANA identifier.",
            problemDetails.Detail);

        Assert.Equal(
            "/api/auth/register",
            problemDetails.Instance);
    }

    [Fact]
    public async Task Register_WithDuplicateAccount_ReturnsConflictProblemDetails()
    {
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

        var firstCsrfResponse =
            await _client.GetAsync("/api/auth/csrf-token");

        firstCsrfResponse.EnsureSuccessStatusCode();

        var firstCsrfResponseBody =
            await firstCsrfResponse.Content
                .ReadFromJsonAsync<AntiforgeryTokenResponse>();

        Assert.NotNull(firstCsrfResponseBody);

        using var firstRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/auth/register")
            {
                Content =
                    JsonContent.Create(registerRequest)
            };

        firstRequest.Headers.Add(
            "X-CSRF-TOKEN",
            firstCsrfResponseBody.RequestToken);

        var firstResponse =
            await _client.SendAsync(firstRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            firstResponse.StatusCode);

        var secondCsrfResponse =
            await _client.GetAsync("/api/auth/csrf-token");

        secondCsrfResponse.EnsureSuccessStatusCode();

        var secondCsrfResponseBody =
            await secondCsrfResponse.Content
                .ReadFromJsonAsync<AntiforgeryTokenResponse>();

        Assert.NotNull(secondCsrfResponseBody);

        using var secondRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/auth/register")
            {
                Content =
                    JsonContent.Create(registerRequest)
            };

        secondRequest.Headers.Add(
            "X-CSRF-TOKEN",
            secondCsrfResponseBody.RequestToken);

        var secondResponse =
            await _client.SendAsync(secondRequest);

        Assert.Equal(
            HttpStatusCode.Conflict,
            secondResponse.StatusCode);

        var problemDetails =
            await secondResponse.Content
                .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problemDetails);

        Assert.Equal(
            StatusCodes.Status409Conflict,
            problemDetails.Status);

        Assert.Equal(
            "Account conflict",
            problemDetails.Title);

        Assert.Equal(
            "An account with the supplied email or username already exists.",
            problemDetails.Detail);

        Assert.Equal(
            "/api/auth/register",
            problemDetails.Instance);
    }
}
