using HabitTracker.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Api.ExceptionHandling;

public sealed class ApiExceptionHandler
    : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    public ApiExceptionHandler(
        IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails =
            exception switch
            {
                InvalidIanaTimeZoneException =>
                    new ProblemDetails
                    {
                        Status =
                            StatusCodes.Status400BadRequest,
                        Title =
                            "Invalid time zone",
                        Detail =
                            exception.Message
                    },

                AccountConflictException =>
                    new ProblemDetails
                    {
                        Status =
                            StatusCodes.Status409Conflict,
                        Title =
                            "Account conflict",
                        Detail =
                            exception.Message
                    },

                InvalidCredentialsException =>
                    new ProblemDetails
                    {
                        Status =
                            StatusCodes.Status401Unauthorized,
                        Title =
                            "Invalid credentials",
                        Detail =
                            exception.Message
                    },
                InvalidHabitNameException =>
                    new ProblemDetails
                    {
                        Status =
                            StatusCodes.Status400BadRequest,
                        Title =
                            "Invalid habit name",
                        Detail =
                            exception.Message
                    },

                InvalidHabitTargetCountException =>
                    new ProblemDetails
                    {
                        Status =
                            StatusCodes.Status400BadRequest,
                        Title =
                            "Invalid habit target count",
                        Detail =
                            exception.Message
                    },

                _ => null
            };

        if (problemDetails is null)
        {
            return false;
        }

        problemDetails.Instance =
            httpContext.Request.Path.Value;

        httpContext.Response.StatusCode =
            problemDetails.Status!.Value;

        return await _problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception
            });
    }
}
