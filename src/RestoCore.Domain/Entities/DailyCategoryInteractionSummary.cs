namespace RestoCore.Domain.Entities;

using RestoCore.Domain.Common;

public class DailyCategoryInteractionSummary : BaseEntity, ITenantScopedEntity
{
    public Guid TenantId { get; set; }
    public Guid CategoryId { get; set; }
    public DateOnly DateUtc { get; set; }
    public int InteractionsCount { get; set; }

    public Tenant? Tenant { get; set; }
    public Category? Category { get; set; }
}
