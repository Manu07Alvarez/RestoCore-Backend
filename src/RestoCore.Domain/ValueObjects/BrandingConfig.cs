namespace RestoCore.Domain.ValueObjects;

using System.Globalization;
using System.Text.RegularExpressions;

public partial class BrandingConfig
{
    private static readonly Regex RgbFuncRegex = MyRgbFuncRegex();
    private static readonly Regex RgbTupleRegex = MyRgbTupleRegex();
    private static readonly Regex HexColorRegex = MyHexColorRegex();

    public string PrimaryColor { get; set; } = "#E63946";
    public string PrimaryColorRgb { get; set; } = "rgb(230, 57, 70)";
    public string SecondaryColor { get; set; } = "#1D3557";
    public string BackgroundColor { get; set; } = "#F8F9FA";
    public string TextColor { get; set; } = "#2B2D42";
    public string? LogoUrl { get; set; }
    public string? CoverBannerUrl { get; set; }
    public string FontFamily { get; set; } = "Inter, sans-serif";
    public string LayoutMode { get; set; } = "GridWithImages";

    /// <summary>
    /// Applies a brand color provided either in RGB format ("rgb(r, g, b)" or "r, g, b")
    /// or Hex format ("#RRGGBB" / "#RGB"), synchronizing both PrimaryColorRgb and PrimaryColor.
    /// </summary>
    public void ApplyRgbColor(string colorInput)
    {
        if (string.IsNullOrWhiteSpace(colorInput))
        {
            throw new ArgumentException("El color de branding no puede estar vacío.", nameof(colorInput));
        }

        var trimmed = colorInput.Trim();
        var (r, g, b) = ParseToRgbComponents(trimmed);

        PrimaryColorRgb = $"rgb({r}, {g}, {b})";
        PrimaryColor = $"#{r:X2}{g:X2}{b:X2}";
    }

    public static (int R, int G, int B) ParseToRgbComponents(string colorInput)
    {
        if (string.IsNullOrWhiteSpace(colorInput))
        {
            throw new ArgumentException("El color no puede estar vacío.", nameof(colorInput));
        }

        var trimmed = colorInput.Trim();

        var rgbMatch = RgbFuncRegex.Match(trimmed);
        if (!rgbMatch.Success)
        {
            rgbMatch = RgbTupleRegex.Match(trimmed);
        }

        if (rgbMatch.Success)
        {
            var r = int.Parse(rgbMatch.Groups[1].Value, CultureInfo.InvariantCulture);
            var g = int.Parse(rgbMatch.Groups[2].Value, CultureInfo.InvariantCulture);
            var b = int.Parse(rgbMatch.Groups[3].Value, CultureInfo.InvariantCulture);

            if (r is < 0 or > 255 || g is < 0 or > 255 || b is < 0 or > 255)
            {
                throw new ArgumentException("Los valores RGB deben estar en el rango de 0 a 255.", nameof(colorInput));
            }

            return (r, g, b);
        }

        var hexMatch = HexColorRegex.Match(trimmed);
        if (hexMatch.Success)
        {
            var hex = hexMatch.Groups[1].Value;
            if (hex.Length == 3)
            {
                var r = int.Parse(new string(hex[0], 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                var g = int.Parse(new string(hex[1], 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                var b = int.Parse(new string(hex[2], 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                return (r, g, b);
            }

            var r6 = int.Parse(hex[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            var g6 = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            var b6 = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return (r6, g6, b6);
        }

        throw new ArgumentException(
            "Formato de color inválido. Use formato RGB ('rgb(230, 57, 70)' o '230, 57, 70') o Hex ('#E63946').",
            nameof(colorInput));
    }

    [GeneratedRegex(@"^rgb\(\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(\d{1,3})\s*\)$", RegexOptions.IgnoreCase)]
    private static partial Regex MyRgbFuncRegex();

    [GeneratedRegex(@"^(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(\d{1,3})$")]
    private static partial Regex MyRgbTupleRegex();

    [GeneratedRegex(@"^#?([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$")]
    private static partial Regex MyHexColorRegex();
}
