namespace RestoCore.Domain.ValueObjects;

public class LayoutConfig
{
    public bool CanvasEnabled { get; set; }
    public string? BackgroundUrl { get; set; }
    public string? BackgroundColor { get; set; }
    public List<CanvasElement> Elements { get; set; } = new();
}