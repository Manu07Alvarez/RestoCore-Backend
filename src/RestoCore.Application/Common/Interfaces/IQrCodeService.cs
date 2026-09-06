namespace RestoCore.Application.Common.Interfaces;

public interface IQrCodeService
{
    string GenerateSvg(string payloadUrl);
    byte[] GeneratePng(string payloadUrl, int pixelsPerModule = 20);
}
