using HabitTracker.Api.Controllers;
using HabitTracker.Api.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Tests.Controllers;

public sealed class HealthControllerTests
{
    [Fact]
    public void GetHealth_ReturnsOkResponseWithHealthyStatus()
    {
        var beforeCallUtc = DateTime.UtcNow;
        var controller = new HealthController();

        var actionResult = controller.GetHealth();

        var okObjectResult = Assert.IsType<OkObjectResult>(actionResult.Result);

        var healthResponse = Assert.IsType<HealthResponse>(okObjectResult.Value);

        Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        Assert.Equal("Healthy", healthResponse.Status);
        Assert.InRange(healthResponse.CheckedAtUtc, beforeCallUtc, DateTime.UtcNow);
    }
}
