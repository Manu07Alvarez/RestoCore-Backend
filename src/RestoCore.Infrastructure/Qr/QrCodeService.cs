namespace RestoCore.Infrastructure.Qr;

using QRCoder;
using RestoCore.Application.Common.Interfaces;

public class QrCodeService : IQrCodeService
{
    public string GenerateSvg(string payloadUrl)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(payloadUrl, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new SvgQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }

    public byte[] GeneratePng(string payloadUrl, int pixelsPerModule = 20)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(payloadUrl, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(pixelsPerModule);
    }
}
