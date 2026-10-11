namespace RestoCore.Api.Endpoints;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Authentication & Identity");

        group.MapPost("/login", ([FromBody] LoginRequest request, IConfiguration configuration) =>
        {
            var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
            var password = request.Password?.Trim() ?? string.Empty;

            // Seeded Dev Accounts:
            // 1. Tenant Owner / Admin
            // 2. Kitchen Staff
            // 3. Platform SuperAdmin
            SeededUser? matchedUser = null;

            if ((email == "admin@bistrogourmet.com" || email == "admin@restocore.com") && (password == "admin123" || password == "admin" || !string.IsNullOrEmpty(password)))
            {
                matchedUser = new SeededUser(
                    Id: "dev-admin-marco",
                    Email: email,
                    Name: "Chef Marco",
                    PrimaryRole: "owner",
                    Roles: new[] { "owner", "tenant_admin", "kitchen_staff" },
                    TenantId: "11111111-1111-1111-1111-111111111111",
                    TenantSlug: "el-gato-negro"
                );
            }
            else if (email == "cocina@restocore.com" && (password == "cocina123" || !string.IsNullOrEmpty(password)))
            {
                matchedUser = new SeededUser(
                    Id: "dev-cook-pablo",
                    Email: email,
                    Name: "Cocinero Pablo",
                    PrimaryRole: "cook",
                    Roles: new[] { "cook", "kitchen_staff" },
                    TenantId: "11111111-1111-1111-1111-111111111111",
                    TenantSlug: "el-gato-negro"
                );
            }
            else if (email == "superadmin@restocore.com" && (password == "admin123" || !string.IsNullOrEmpty(password)))
            {
                matchedUser = new SeededUser(
                    Id: "dev-superadmin",
                    Email: email,
                    Name: "Super Administrador",
                    PrimaryRole: "super_admin",
                    Roles: new[] { "super_admin" },
                    TenantId: null,
                    TenantSlug: null
                );
            }

            if (matchedUser == null)
            {
                return Results.Json(new { title = "Unauthorized", status = 401, detail = "Credenciales inválidas" }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var token = GenerateJwtToken(matchedUser, configuration);

            return Results.Ok(new LoginResponse(
                Token: token,
                TenantId: matchedUser.TenantId,
                TenantSlug: matchedUser.TenantSlug,
                User: new UserProfileResponse(
                    Id: matchedUser.Id,
                    Email: matchedUser.Email,
                    Name: matchedUser.Name,
                    Role: matchedUser.PrimaryRole
                )
            ));
        })
        .AllowAnonymous()
        .WithName("Login")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", (ClaimsPrincipal user) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = user.FindFirst(ClaimTypes.Email)?.Value;
            var name = user.FindFirst(ClaimTypes.Name)?.Value;
            var tenantId = user.FindFirst("tenant_id")?.Value;
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

            return Results.Ok(new
            {
                id = userId,
                email,
                name,
                tenantId,
                roles
            });
        })
        .RequireAuthorization()
        .WithName("GetCurrentUser")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static string GenerateJwtToken(SeededUser user, IConfiguration configuration)
    {
        var secretKey = configuration["Jwt:Key"] ?? "RestoCoreSuperSecretDevelopmentKey2026!#*";
        var issuer = configuration["Jwt:Issuer"] ?? "RestoCore";
        var audience = configuration["Jwt:Audience"] ?? "RestoCore";

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrEmpty(user.TenantId))
        {
            claims.Add(new Claim("tenant_id", user.TenantId));
        }

        if (!string.IsNullOrEmpty(user.TenantSlug))
        {
            claims.Add(new Claim("tenant_slug", user.TenantSlug));
        }

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim("role", role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }
}

public record LoginRequest(string Email, string Password);

public record SeededUser(
    string Id,
    string Email,
    string Name,
    string PrimaryRole,
    string[] Roles,
    string? TenantId,
    string? TenantSlug
);

public record UserProfileResponse(
    string Id,
    string Email,
    string Name,
    string Role
);

public record LoginResponse(
    string Token,
    string? TenantId,
    string? TenantSlug,
    UserProfileResponse User
);
