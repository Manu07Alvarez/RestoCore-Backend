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
        var tenantContext = httpContext.RequestServices.GetService<RestoCore.Application.Common.Interfaces.ITenantContext>();
        var (statusCode, title, detail, errorCode) = exception switch
        {
            FluentValidation.ValidationException valEx => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                string.Join("; ", valEx.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")),
                "VALIDATION_ERROR"
            ),
            KeyNotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                notFoundEx.Message,
                "RESOURCE_NOT_FOUND"
            ),
            UnauthorizedAccessException authEx => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                authEx.Message,
                "FORBIDDEN"
            ),
            InvalidOperationException opEx => (
                StatusCodes.Status409Conflict,
                "Conflict",
                opEx.Message,
                "CONFLICT"
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred while processing your request.",
                "INTERNAL_SERVER_ERROR"
            )
        };

        _logger.LogError(exception, 
            "Unhandled exception occurred: {Message}. ErrorCode: {ErrorCode}, Tenant: {TenantSlug}, TenantId: {TenantId}, StatusCode: {StatusCode}", 
            exception.Message,
            errorCode,
            tenantContext?.TenantSlug ?? "none",
            tenantContext?.TenantId?.ToString() ?? "none",
            statusCode);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        problemDetails.Extensions["traceId"] = traceId;
        problemDetails.Extensions["errorCode"] = errorCode;

        if (tenantContext?.TenantId.HasValue == true)
        {
            problemDetails.Extensions["tenantId"] = tenantContext.TenantId.Value;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
