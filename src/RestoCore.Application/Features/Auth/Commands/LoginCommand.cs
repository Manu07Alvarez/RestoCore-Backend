namespace RestoCore.Application.Features.Auth.Commands;

using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Common.Mediator;

public record LoginCommand(string Email, string Password) : IRequest<AuthenticatedUserResult>;

public record AuthenticatedUserResult(
    string Id,
    string Email,
    string Name,
    string PrimaryRole,
    string[] Roles,
    string? TenantId,
    string? TenantSlug
);

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthenticatedUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthenticatedUserResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var password = request.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        var user = await _userRepository.GetUserByEmailWithTenantAsync(email, cancellationToken);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        return new AuthenticatedUserResult(
            Id: user.Id.ToString(),
            Email: user.Email,
            Name: user.Name,
            PrimaryRole: user.PrimaryRole,
            Roles: user.Roles,
            TenantId: user.TenantId?.ToString(),
            TenantSlug: user.Tenant?.Slug
        );
    }
}
