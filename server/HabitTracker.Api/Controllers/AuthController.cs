using System.Security.Claims;
using HabitTracker.Api.DTOs;
using HabitTracker.Api.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAntiforgery _antiforgery;
    private readonly AuthService _authService;

    public AuthController(
        IAntiforgery antiforgery,
        AuthService authService)
    {
        _antiforgery = antiforgery;
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpGet("csrf-token")]
    public ActionResult<AntiforgeryTokenResponse> GetCsrfToken()
    {
        var tokens =
            _antiforgery.GetAndStoreTokens(HttpContext);

        var requestToken =
            tokens.RequestToken
            ?? throw new InvalidOperationException(
                "The antiforgery request token could not be generated.");

        return Ok(
            new AntiforgeryTokenResponse
            {
                RequestToken = requestToken
            });
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var authResponse =
            await _authService.RegisterAsync(
                request,
                cancellationToken);

        await SignInUserAsync(
            authResponse.User.Id,
            rememberMe: false);

        return StatusCode(
            StatusCodes.Status201Created,
            authResponse);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var authResponse =
            await _authService.LoginAsync(
                request,
                cancellationToken);

        await SignInUserAsync(
            authResponse.User.Id,
            request.RememberMe);

        return Ok(authResponse);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return NoContent();
    }

    private async Task SignInUserAsync(
        Guid userId,
        bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                userId.ToString())
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        var expiresUtc =
            rememberMe
                ? DateTimeOffset.UtcNow.AddDays(30)
                : DateTimeOffset.UtcNow.AddHours(12);

        var authenticationProperties =
            new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = expiresUtc
            };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authenticationProperties);
    }
}
