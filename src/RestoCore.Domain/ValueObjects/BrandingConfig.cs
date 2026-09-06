namespace RestoCore.Domain.ValueObjects;

public class BrandingConfig
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
