using System.Security.Claims;
using HabitTracker.Api.DTOs;
using HabitTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/habits")]
public sealed class HabitsController : ControllerBase
{
    private readonly HabitService _habitService;

    public HabitsController(HabitService habitService)
    {
        _habitService = habitService;
    }

    [HttpPost]
    public async Task<ActionResult<HabitResponse>> CreateHabitAsync(
        CreateHabitRequest request,
        CancellationToken cancellationToken)
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
            userIdValue,
            out var userId))
        {
            return Unauthorized();
        }

        var habit =
            await _habitService.CreateHabitAsync(
                userId,
                request,
                cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            habit);
    }
}