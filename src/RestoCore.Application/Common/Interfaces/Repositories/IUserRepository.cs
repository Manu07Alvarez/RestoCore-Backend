namespace RestoCore.Application.Common.Interfaces.Repositories;

using RestoCore.Domain.Entities;

public interface IUserRepository
{
    Task<User?> GetUserByEmailWithTenantAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    void AddUser(User user);
}
