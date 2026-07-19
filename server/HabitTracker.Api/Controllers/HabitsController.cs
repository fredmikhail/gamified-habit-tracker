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

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HabitResponse>>> GetUserHabitsAsync(
    [FromQuery] bool includeInactive = false,
    CancellationToken cancellationToken = default)
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

        var habits =
            await _habitService.GetUserHabitsAsync(
                userId,
                includeInactive,
                cancellationToken);

        return Ok(habits);
    }

    [HttpGet("{habitId:guid}")]
    public async Task<ActionResult<HabitResponse>> GetUserHabitAsync(
        Guid habitId,
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
            await _habitService.GetUserHabitAsync(
                userId,
                habitId,
                cancellationToken);

        if (habit is null)
        {
            return NotFound();
        }

        return Ok(habit);
    }

    [HttpPut("{habitId:guid}")]
    public async Task<ActionResult<HabitResponse>> UpdateHabitAsync(
    Guid habitId,
    UpdateHabitRequest request,
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
            await _habitService.UpdateHabitAsync(
                userId,
                habitId,
                request,
                cancellationToken);

        if (habit is null)
        {
            return NotFound();
        }

        return Ok(habit);
    }

    [HttpDelete("{habitId:guid}")]
    public async Task<ActionResult<HabitResponse>> DeactivateHabitAsync(
    Guid habitId,
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
            await _habitService.DeactivateHabitAsync(
                userId,
                habitId,
                cancellationToken);

        if (habit is null)
        {
            return NotFound();
        }

        return Ok(habit);
    }
}