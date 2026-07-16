using System.Net;
using System.Net.Http.Json;
using HabitTracker.Api.DTOs;
using HabitTracker.IntegrationTests.Infrastructure;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.IntegrationTests.Controllers;

public sealed class AuthControllerTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AuthControllerTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;

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

    [Fact]
    public async Task Login_WithoutRememberMe_ReturnsOkAndSetsSessionCookie()
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

        var registrationCsrfResponse =
            await _client.GetAsync("/api/auth/csrf-token");

        registrationCsrfResponse.EnsureSuccessStatusCode();

        var registrationCsrfBody =
            await registrationCsrfResponse.Content
                .ReadFromJsonAsync<AntiforgeryTokenResponse>();

        Assert.NotNull(registrationCsrfBody);

        using var registrationRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/auth/register")
            {
                Content =
                    JsonContent.Create(registerRequest)
            };

        registrationRequest.Headers.Add(
            "X-CSRF-TOKEN",
            registrationCsrfBody.RequestToken);

        var registrationResponse =
            await _client.SendAsync(registrationRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            registrationResponse.StatusCode);

        var registrationResponseBody =
            await registrationResponse.Content
                .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(registrationResponseBody);

        using var loginClient =
            _factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false,
                    HandleCookies = true
                });

        var loginCsrfResponse =
            await loginClient.GetAsync("/api/auth/csrf-token");

        loginCsrfResponse.EnsureSuccessStatusCode();

        var loginCsrfBody =
            await loginCsrfResponse.Content
                .ReadFromJsonAsync<AntiforgeryTokenResponse>();

        Assert.NotNull(loginCsrfBody);

        var loginRequest =
            new LoginRequest
            {
                Email = registerRequest.Email,
                Password = registerRequest.Password,
                RememberMe = false
            };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/auth/login")
            {
                Content =
                    JsonContent.Create(loginRequest)
            };

        request.Headers.Add(
            "X-CSRF-TOKEN",
            loginCsrfBody.RequestToken);

        var response =
            await loginClient.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var responseBody =
            await response.Content
                .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(responseBody);

        Assert.Equal(
            registrationResponseBody.User.Id,
            responseBody.User.Id);

        Assert.Equal(
            registerRequest.Email,
            responseBody.User.Email);

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
    public async Task Login_WithRememberMe_ReturnsOkAndSetsPersistentCookie()
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

        var registrationCsrfResponse =
            await _client.GetAsync("/api/auth/csrf-token");

        registrationCsrfResponse.EnsureSuccessStatusCode();

        var registrationCsrfBody =
            await registrationCsrfResponse.Content
                .ReadFromJsonAsync<AntiforgeryTokenResponse>();

        Assert.NotNull(registrationCsrfBody);

        using var registrationRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/auth/register")
            {
                Content =
                    JsonContent.Create(registerRequest)
            };

        registrationRequest.Headers.Add(
            "X-CSRF-TOKEN",
            registrationCsrfBody.RequestToken);

        var registrationResponse =
            await _client.SendAsync(registrationRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            registrationResponse.StatusCode);

        var registrationResponseBody =
            await registrationResponse.Content
                .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(registrationResponseBody);

        using var loginClient =
            _factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false,
                    HandleCookies = true
                });

        var loginCsrfResponse =
            await loginClient.GetAsync("/api/auth/csrf-token");

        loginCsrfResponse.EnsureSuccessStatusCode();

        var loginCsrfBody =
            await loginCsrfResponse.Content
                .ReadFromJsonAsync<AntiforgeryTokenResponse>();

        Assert.NotNull(loginCsrfBody);

        var loginRequest =
            new LoginRequest
            {
                Email = registerRequest.Email,
                Password = registerRequest.Password,
                RememberMe = true
            };

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/auth/login")
            {
                Content =
                    JsonContent.Create(loginRequest)
            };

        request.Headers.Add(
            "X-CSRF-TOKEN",
            loginCsrfBody.RequestToken);

        var beforeLoginUtc =
            DateTimeOffset.UtcNow;

        var response =
            await loginClient.SendAsync(request);

        var afterLoginUtc =
            DateTimeOffset.UtcNow;

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var responseBody =
            await response.Content
                .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(responseBody);

        Assert.Equal(
            registrationResponseBody.User.Id,
            responseBody.User.Id);

        Assert.Equal(
            registerRequest.Email,
            responseBody.User.Email);

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

        var parsedAuthenticationCookie =
            SetCookieHeaderValue.Parse(
                authenticationCookie);

        Assert.NotNull(
            parsedAuthenticationCookie.Expires);

        Assert.InRange(
            parsedAuthenticationCookie.Expires.Value,
            beforeLoginUtc
                .AddDays(30)
                .AddMinutes(-1),
            afterLoginUtc
                .AddDays(30)
                .AddMinutes(1));
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsGenericUnauthorizedProblemDetails()
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

        var registrationCsrfResponse =
            await _client.GetAsync("/api/auth/csrf-token");

        registrationCsrfResponse.EnsureSuccessStatusCode();

        var registrationCsrfBody =
            await registrationCsrfResponse.Content
                .ReadFromJsonAsync<AntiforgeryTokenResponse>();

        Assert.NotNull(registrationCsrfBody);

        using var registrationRequest =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/auth/register")
            {
                Content =
                    JsonContent.Create(registerRequest)
            };

        registrationRequest.Headers.Add(
            "X-CSRF-TOKEN",
            registrationCsrfBody.RequestToken);

        var registrationResponse =
            await _client.SendAsync(registrationRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            registrationResponse.StatusCode);

        using var loginClient =
            _factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false,
                    HandleCookies = true
                });

        var wrongEmailRequest =
            new LoginRequest
            {
                Email =
                    $"missing_{uniqueSuffix}@example.com",
                Password =
                    registerRequest.Password,
                RememberMe = false
            };

        var wrongEmailResponse =
            await SendLoginRequestAsync(
                loginClient,
                wrongEmailRequest);

        var wrongPasswordRequest =
            new LoginRequest
            {
                Email =
                    registerRequest.Email,
                Password =
                    "WrongPassword123!",
                RememberMe = false
            };

        var wrongPasswordResponse =
            await SendLoginRequestAsync(
                loginClient,
                wrongPasswordRequest);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            wrongEmailResponse.StatusCode);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            wrongPasswordResponse.StatusCode);

        var wrongEmailProblemDetails =
            await wrongEmailResponse.Content
                .ReadFromJsonAsync<ProblemDetails>();

        var wrongPasswordProblemDetails =
            await wrongPasswordResponse.Content
                .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(wrongEmailProblemDetails);
        Assert.NotNull(wrongPasswordProblemDetails);

        Assert.Equal(
            "Invalid credentials",
            wrongEmailProblemDetails.Title);

        Assert.Equal(
            "Invalid credentials",
            wrongPasswordProblemDetails.Title);

        Assert.Equal(
            "The supplied email or password is incorrect.",
            wrongEmailProblemDetails.Detail);

        Assert.Equal(
            wrongEmailProblemDetails.Detail,
            wrongPasswordProblemDetails.Detail);

        Assert.Equal(
            StatusCodes.Status401Unauthorized,
            wrongEmailProblemDetails.Status);

        Assert.Equal(
            StatusCodes.Status401Unauthorized,
            wrongPasswordProblemDetails.Status);

        Assert.Equal(
            "/api/auth/login",
            wrongEmailProblemDetails.Instance);

        Assert.Equal(
            "/api/auth/login",
            wrongPasswordProblemDetails.Instance);
    }

    private static async Task<HttpResponseMessage> SendLoginRequestAsync(
        HttpClient client,
        LoginRequest loginRequest)
    {
        var csrfResponse =
            await client.GetAsync("/api/auth/csrf-token");

        csrfResponse.EnsureSuccessStatusCode();

        var csrfResponseBody =
            await csrfResponse.Content
                .ReadFromJsonAsync<AntiforgeryTokenResponse>();

        Assert.NotNull(csrfResponseBody);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "/api/auth/login")
            {
                Content =
                    JsonContent.Create(loginRequest)
            };

        request.Headers.Add(
            "X-CSRF-TOKEN",
            csrfResponseBody.RequestToken);

        return await client.SendAsync(request);
    }
}
