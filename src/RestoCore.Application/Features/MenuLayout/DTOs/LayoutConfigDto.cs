namespace RestoCore.Application.Features.MenuLayout.DTOs;

public class CanvasElementDto
{
    public Guid DishId { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public int ZIndex { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
}

public class LayoutConfigDto
{
    public bool CanvasEnabled { get; set; }
    public string? BackgroundUrl { get; set; }
    public string? BackgroundColor { get; set; }
    public List<CanvasElementDto> Elements { get; set; } = new();
}