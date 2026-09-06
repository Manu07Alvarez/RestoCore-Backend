namespace RestoCore.Application.Common.Interfaces;

public interface ITenantContext
{
    Guid? TenantId { get; }
    string? TenantSlug { get; }
    bool HasTenant => TenantId.HasValue;
    void SetTenant(Guid tenantId, string tenantSlug);
}
