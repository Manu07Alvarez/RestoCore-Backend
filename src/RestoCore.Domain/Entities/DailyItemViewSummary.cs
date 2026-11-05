namespace RestoCore.Domain.Entities;

using RestoCore.Domain.Common;

public class DailyItemViewSummary : BaseEntity, ITenantScopedEntity
{
    public Guid TenantId { get; set; }
    public Guid ItemId { get; set; }
    public DateOnly DateUtc { get; set; }
    public int ViewsCount { get; set; }

    public Tenant? Tenant { get; set; }
    public MenuItem? Item { get; set; }
}
