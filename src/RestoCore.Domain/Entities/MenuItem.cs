namespace RestoCore.Domain.Entities;

using RestoCore.Domain.Common;

public class MenuItem : BaseEntity, ITenantScopedEntity
{
    public Guid TenantId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public string[] Allergens { get; set; } = Array.Empty<string>();
    public string[] DietaryLabels { get; set; } = Array.Empty<string>();

    public Tenant? Tenant { get; set; }
    public Category? Category { get; set; }
    public ICollection<ModifierGroup> ModifierGroups { get; set; } = new List<ModifierGroup>();
}
