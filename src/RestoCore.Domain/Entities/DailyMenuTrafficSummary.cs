namespace RestoCore.Domain.Entities;

using RestoCore.Domain.Common;

public class DailyMenuTrafficSummary : BaseEntity, ITenantScopedEntity
{
    public Guid TenantId { get; set; }
    public DateOnly DateUtc { get; set; }
    public int QrVisitsCount { get; set; }
    public int MobileCount { get; set; }
    public int DesktopCount { get; set; }
    public int TabletCount { get; set; }

    public Tenant? Tenant { get; set; }
}
