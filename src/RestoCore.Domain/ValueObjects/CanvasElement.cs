namespace RestoCore.Domain.ValueObjects;

public class CanvasElement
{
    public Guid DishId { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public int ZIndex { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
}