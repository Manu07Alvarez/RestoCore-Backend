namespace RestoCore.Infrastructure.Authorization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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

        var permissions = ResolveEffectivePermissions(context.User, userRoles);

        var input = new OpaInput
        {
            Method = httpContext.Request.Method,
            Path = pathSegments,
            Action = requirement.Action,
            User = new OpaUser
            {
                Id = userId,
                TenantId = userTenantId,
                Roles = userRoles,
                Permissions = permissions
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

    private static string[] ResolveEffectivePermissions(ClaimsPrincipal user, string[] roles)
    {
        var permissionSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var claim in user.FindAll("permission"))
        {
            if (!string.IsNullOrWhiteSpace(claim.Value))
            {
                permissionSet.Add(claim.Value.Trim());
            }
        }

        foreach (var role in roles)
        {
            switch (role.Trim().ToLowerInvariant())
            {
                case "superadmin":
                    permissionSet.Add("manage_tenants");
                    permissionSet.Add("manage_menu");
                    permissionSet.Add("manage_categories");
                    permissionSet.Add("manage_branding");
                    permissionSet.Add("kitchen_stock");
                    permissionSet.Add("read_metrics");
                    permissionSet.Add("generate_presigned_url");
                    break;
                case "owner":
                case "admin":
                    permissionSet.Add("manage_menu");
                    permissionSet.Add("manage_categories");
                    permissionSet.Add("manage_branding");
                    permissionSet.Add("kitchen_stock");
                    permissionSet.Add("read_metrics");
                    permissionSet.Add("generate_presigned_url");
                    break;
                case "cook":
                    permissionSet.Add("kitchen_stock");
                    break;
                case "waiter":
                    permissionSet.Add("read_menu");
                    break;
            }
        }

        return permissionSet.ToArray();
    }
}
