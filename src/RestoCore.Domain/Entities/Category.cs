namespace RestoCore.Domain.Entities;

using RestoCore.Domain.Common;

public class Category : BaseEntity, ITenantScopedEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant? Tenant { get; set; }
    public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
}
