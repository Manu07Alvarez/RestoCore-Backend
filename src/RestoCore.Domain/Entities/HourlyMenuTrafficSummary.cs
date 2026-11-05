namespace RestoCore.Domain.Entities;

using RestoCore.Domain.Common;

public class HourlyMenuTrafficSummary : BaseEntity, ITenantScopedEntity
{
    public Guid TenantId { get; set; }
    public DateOnly DateUtc { get; set; }
    public int HourOfDay { get; set; }
    public int VisitsCount { get; set; }

    public Tenant? Tenant { get; set; }
}
