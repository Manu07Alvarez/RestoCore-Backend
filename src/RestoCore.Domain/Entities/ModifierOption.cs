namespace RestoCore.Domain.Entities;

using RestoCore.Domain.Common;

public class ModifierOption : BaseEntity, ITenantScopedEntity
{
    public Guid TenantId { get; set; }
    public Guid ModifierGroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PriceDelta { get; set; }
    public bool IsAvailable { get; set; } = true;

    public ModifierGroup? ModifierGroup { get; set; }
}
