namespace RestoCore.Domain.Entities;

using RestoCore.Domain.Common;

public class Table : BaseEntity, ITenantScopedEntity
{
    public Guid TenantId { get; set; }
    public int TableNumber { get; set; }
    public string Token { get; set; } = Guid.NewGuid().ToString("N");
    public string? Label { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant? Tenant { get; set; }
}
