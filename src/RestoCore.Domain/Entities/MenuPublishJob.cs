namespace RestoCore.Domain.Entities;

using RestoCore.Domain.Common;

public class MenuPublishJob : BaseEntity, ITenantScopedEntity
{
    public Guid TenantId { get; set; }
    public string TenantSlug { get; set; } = string.Empty;
    public string Status { get; set; } = "queued";
    public string VersionHash { get; set; } = string.Empty;
    public bool CdnPurgeRequested { get; set; }
    public int AssetsQueued { get; set; }
    public int EstimatedDurationSeconds { get; set; } = 3;
    public DateTimeOffset TriggeredAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }

    public Tenant? Tenant { get; set; }
}