using System.Security.Claims;
using HabitTracker.Api.DTOs;
using HabitTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/attributes")]
public sealed class AttributesController : ControllerBase
{
    private readonly AttributeService _attributeService;

    public AttributesController(
        AttributeService attributeService)
    {
        _attributeService = attributeService;
    }

    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyList<UserAttributeResponse>>>
        GetUserAttributesAsync(
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

        var attributes =
            await _attributeService
                .GetUserAttributesAsync(
                    userId,
                    cancellationToken);

        return Ok(attributes);
    }
}
