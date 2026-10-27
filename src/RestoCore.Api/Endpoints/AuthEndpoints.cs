namespace RestoCore.Api.Endpoints;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Features.Auth.Commands;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Authentication & Identity");

        group.MapPost("/login", async ([FromBody] LoginRequest request, ISender sender, IConfiguration configuration, CancellationToken cancellationToken) =>
        {
            try
            {
                var authenticatedUser = await sender.Send(
                    new LoginCommand(request.Email ?? string.Empty, request.Password ?? string.Empty),
                    cancellationToken);

                var token = GenerateJwtToken(authenticatedUser, configuration);

                return Results.Ok(new LoginResponse(
                    Token: token,
                    TenantId: authenticatedUser.TenantId,
                    TenantSlug: authenticatedUser.TenantSlug,
                    User: new UserProfileResponse(
                        Id: authenticatedUser.Id,
                        Email: authenticatedUser.Email,
                        Name: authenticatedUser.Name,
                        Role: authenticatedUser.PrimaryRole
                    )
                ));
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Json(
                    new { title = "Unauthorized", status = 401, detail = "Credenciales inválidas" },
                    statusCode: StatusCodes.Status401Unauthorized);
            }
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

    private static string GenerateJwtToken(AuthenticatedUserResult user, IConfiguration configuration)
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
