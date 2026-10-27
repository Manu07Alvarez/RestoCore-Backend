namespace RestoCore.Domain.Entities;

using RestoCore.Domain.Common;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PrimaryRole { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
    public Guid? TenantId { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant? Tenant { get; set; }
}
