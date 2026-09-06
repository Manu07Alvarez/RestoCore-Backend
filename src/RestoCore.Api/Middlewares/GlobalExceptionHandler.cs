namespace RestoCore.Api.Middlewares;

using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        var (statusCode, title, detail) = exception switch
        {
            FluentValidation.ValidationException valEx => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                string.Join("; ", valEx.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))
            ),
            KeyNotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                notFoundEx.Message
            ),
            UnauthorizedAccessException authEx => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                authEx.Message
            ),
            InvalidOperationException opEx => (
                StatusCodes.Status409Conflict,
                "Conflict",
                opEx.Message
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred while processing your request."
            )
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        problemDetails.Extensions["traceId"] = traceId;

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
