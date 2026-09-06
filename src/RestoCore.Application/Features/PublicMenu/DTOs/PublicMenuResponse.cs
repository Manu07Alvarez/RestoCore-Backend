namespace RestoCore.Application.Features.PublicMenu.DTOs;

public class PublicMenuResponse
{
    public string TenantName { get; set; } = string.Empty;
    public string TenantSlug { get; set; } = string.Empty;
    public int? TableNumber { get; set; }
    public PublicBrandingDto Branding { get; set; } = new();
    public List<PublicCategoryDto> Categories { get; set; } = new();
}

public class PublicBrandingDto
{
    public string PrimaryColor { get; set; } = "#E63946";
    public string SecondaryColor { get; set; } = "#1D3557";
    public string BackgroundColor { get; set; } = "#F8F9FA";
    public string TextColor { get; set; } = "#2B2D42";
    public string? LogoUrl { get; set; }
    public string? CoverBannerUrl { get; set; }
    public string FontFamily { get; set; } = "Inter, sans-serif";
    public string LayoutMode { get; set; } = "GridWithImages";
}

public class PublicCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<PublicMenuItemDto> Items { get; set; } = new();
}

public class PublicMenuItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public string[] Allergens { get; set; } = Array.Empty<string>();
    public string[] DietaryLabels { get; set; } = Array.Empty<string>();
    public List<PublicModifierGroupDto> ModifierGroups { get; set; } = new();
}

public class PublicModifierGroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MinSelection { get; set; }
    public int MaxSelection { get; set; }
    public bool IsRequired { get; set; }
    public List<PublicModifierOptionDto> Options { get; set; } = new();
}

public class PublicModifierOptionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PriceDelta { get; set; }
    public bool IsAvailable { get; set; }
}
