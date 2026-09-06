namespace RestoCore.UnitTests.Qr;

using FluentAssertions;
using RestoCore.Infrastructure.Qr;
using Xunit;

public class QrCodeServiceTests
{
    private readonly QrCodeService _service = new();

    [Fact]
    public void GenerateSvg_WithValidPayload_ReturnsValidSvgString()
    {
        var url = "https://app.restocore.com/r/la-trattoria?table_token=xyz123";

        var svg = _service.GenerateSvg(url);

        svg.Should().NotBeNullOrWhiteSpace();
        svg.Should().Contain("<svg");
        svg.Should().Contain("</svg>");
    }

    [Fact]
    public void GeneratePng_WithValidPayload_ReturnsNonEmptyPngByteArray()
    {
        var url = "https://app.restocore.com/r/la-trattoria?table_token=xyz123";

        var png = _service.GeneratePng(url, 10);

        png.Should().NotBeNull();
        png.Length.Should().BeGreaterThan(0);
        // PNG header magic bytes: 0x89, 0x50, 0x4E, 0x47
        png[0].Should().Be(0x89);
        png[1].Should().Be(0x50);
        png[2].Should().Be(0x4E);
        png[3].Should().Be(0x47);
    }
}
