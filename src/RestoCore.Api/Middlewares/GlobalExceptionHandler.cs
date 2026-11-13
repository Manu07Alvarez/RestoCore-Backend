namespace RestoCore.Api.Middlewares;

using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            ArgumentException argEx => (
                StatusCodes.Status400BadRequest,
                "Bad Request",
                argEx.Message,
                "BAD_REQUEST"
            ),
            KeyNotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                notFoundEx.Message,
                "RESOURCE_NOT_FOUND"
            ),
            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                "You do not have permission to perform this operation.",
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

        var tenantId = tenantContext?.TenantId?.ToString()
            ?? httpContext.Items["TenantId"]?.ToString()
            ?? "none";
        var tenantSlug = tenantContext?.TenantSlug
            ?? httpContext.Items["TenantSlug"]?.ToString()
            ?? "none";

        // ADR-0017 (OPT-07): Sanitized structured error log without dumping raw exception.Message PII
        _logger.LogError(
            "Operation failed. ErrorCode: {ErrorCode}, ExceptionType: {ExceptionType}, StatusCode: {StatusCode}, Tenant: {TenantSlug}, TenantId: {TenantId}",
            errorCode,
            exception.GetType().Name,
            statusCode,
            tenantSlug,
            tenantId);

        var traceId = Activity.Current?.TraceId.ToString() ?? Activity.Current?.Id ?? httpContext.TraceIdentifier;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = $"https://api.restocore.com/errors/{errorCode.ToLowerInvariant()}",
            Instance = httpContext.Request.Path
        };

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
