namespace RestoCore.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Domain.Entities;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByEmailWithTenantAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail && u.IsActive, cancellationToken);
    }

    public async Task<bool> UserExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await _context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);
    }

    public void AddUser(User user)
    {
        _context.Users.Add(user);
    }
}
