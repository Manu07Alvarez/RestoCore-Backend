namespace RestoCore.Application.Features.MenuLayout.DTOs;

public class MenuPublishRequest
{
    public bool ForceRecompile { get; set; } = false;
}

public class MenuPublishJobResponse
{
    public Guid JobId { get; set; }
    public string TenantSlug { get; set; } = string.Empty;
    public string Status { get; set; } = "queued";
    public int EstimatedDurationSeconds { get; set; } = 3;
    public string VersionHash { get; set; } = string.Empty;
    public bool CdnPurgeRequested { get; set; }
    public int AssetsQueued { get; set; }
    public DateTime TriggeredAt { get; set; }
}
