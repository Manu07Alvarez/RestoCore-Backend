namespace RestoCore.Domain.Entities;

using RestoCore.Domain.Common;
using RestoCore.Domain.ValueObjects;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? CustomDomain { get; set; }
    public string Status { get; set; } = "Active";
    public BrandingConfig BrandingConfig { get; set; } = new();
    public LayoutConfig? LayoutConfig { get; set; }
    public string? CurrentVersionHash { get; set; }

    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    public ICollection<Table> Tables { get; set; } = new List<Table>();
    public ICollection<MenuPublishJob> MenuPublishJobs { get; set; } = new List<MenuPublishJob>();
}
