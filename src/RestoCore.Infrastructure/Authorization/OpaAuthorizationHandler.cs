namespace RestoCore.Infrastructure.Authorization;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RestoCore.Application.Common.Interfaces;

public class OpaRequirement : IAuthorizationRequirement
{
    public string Action { get; }
    public string ResourceType { get; }

    public OpaRequirement(string action, string resourceType = "tenant")
    {
        Action = action;
        ResourceType = resourceType;
    }
}

public class OpaAuthorizationHandler : AuthorizationHandler<OpaRequirement>
{
    private readonly IOpaClient _opaClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITenantContext _tenantContext;

    public OpaAuthorizationHandler(IOpaClient opaClient, IHttpContextAccessor httpContextAccessor, ITenantContext tenantContext)
    {
        _opaClient = opaClient;
        _httpContextAccessor = httpContextAccessor;
        _tenantContext = tenantContext;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OpaRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        var pathSegments = httpContext.Request.Path.Value?.Trim('/').Split('/') ?? Array.Empty<string>();
        var userRoles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var userTenantId = context.User.FindFirst("tenant_id")?.Value ?? string.Empty;
        var targetTenantId = _tenantContext.TenantId?.ToString() ?? userTenantId;

        var input = new OpaInput
        {
            Method = httpContext.Request.Method,
            Path = pathSegments,
            Action = requirement.Action,
            User = new OpaUser
            {
                Id = userId,
                TenantId = userTenantId,
                Roles = userRoles
            },
            Resource = new OpaResource
            {
                Type = requirement.ResourceType,
                TenantId = targetTenantId
            }
        };

        var allowed = await _opaClient.EvaluatePolicyAsync(input);
        if (allowed)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}
