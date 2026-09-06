namespace RestoCore.Domain.Entities;

using RestoCore.Domain.Common;

public class ModifierGroup : BaseEntity, ITenantScopedEntity
{
    public Guid TenantId { get; set; }
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MinSelection { get; set; }
    public int MaxSelection { get; set; } = 1;
    public bool IsRequired { get; set; }

    public MenuItem? MenuItem { get; set; }
    public ICollection<ModifierOption> Options { get; set; } = new List<ModifierOption>();
}
