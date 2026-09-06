namespace RestoCore.Infrastructure.MultiTenancy;

using RestoCore.Application.Common.Interfaces;

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }
    public string? TenantSlug { get; private set; }

    public void SetTenant(Guid tenantId, string tenantSlug)
    {
        TenantId = tenantId;
        TenantSlug = tenantSlug;
    }
}
