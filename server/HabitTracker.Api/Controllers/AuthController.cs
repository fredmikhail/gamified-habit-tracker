using HabitTracker.Api.DTOs;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAntiforgery _antiforgery;

    public AuthController(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
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
}
