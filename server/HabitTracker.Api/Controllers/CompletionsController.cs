using System.Security.Claims;
using HabitTracker.Api.DTOs;
using HabitTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/habits/{habitId:guid}/completions")]
public sealed class CompletionsController : ControllerBase
{
    private readonly CompletionService _completionService;

    public CompletionsController(
        CompletionService completionService)
    {
        _completionService = completionService;
    }

    [HttpPost]
    public async Task<ActionResult<CompleteHabitResponse>> CompleteHabitAsync(
        Guid habitId,
        CompleteHabitRequest request,
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

        var response =
            await _completionService.CompleteHabitAsync(
                userId,
                habitId,
                request,
                cancellationToken);

        if (response is null)
        {
            return NotFound();
        }

        return StatusCode(
            StatusCodes.Status201Created,
            response);
    }

    [HttpDelete("today")]
    public async Task<IActionResult> UndoTodayAsync(
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

        var wasRemoved =
            await _completionService.UndoTodayAsync(
                userId,
                habitId,
                cancellationToken);

        if (!wasRemoved)
        {
            return NotFound();
        }

        return NoContent();
    }
}
