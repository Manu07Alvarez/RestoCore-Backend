namespace RestoCore.Application.Features.QrCodes.Queries;

using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoCore.Application.Common.Interfaces;

public record GetTableQrQuery(
    Guid TableId,
    string Format, // "svg" or "png"
    string BaseUrl
) : IRequest<QrResult>;

public record QrResult(
    string ContentType,
    byte[] Data
);

public class GetTableQrQueryHandler : IRequestHandler<GetTableQrQuery, QrResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IQrCodeService _qrCodeService;

    public GetTableQrQueryHandler(
        IApplicationDbContext context,
        ITenantContext tenantContext,
        IQrCodeService qrCodeService)
    {
        _context = context;
        _tenantContext = tenantContext;
        _qrCodeService = qrCodeService;
    }

    public async Task<QrResult> Handle(GetTableQrQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var table = await _context.Tables
            .Include(t => t.Tenant)
            .FirstOrDefaultAsync(t => t.Id == request.TableId && t.TenantId == tenantId, cancellationToken);

        if (table == null)
        {
            throw new KeyNotFoundException($"Table {request.TableId} was not found for current tenant.");
        }

        var tenantSlug = table.Tenant?.Slug ?? "menu";
        var tableUrl = $"{request.BaseUrl.TrimEnd('/')}/r/{tenantSlug}?table_token={table.Token}";

        if (string.Equals(request.Format, "png", StringComparison.OrdinalIgnoreCase))
        {
            var pngBytes = _qrCodeService.GeneratePng(tableUrl);
            return new QrResult("image/png", pngBytes);
        }

        var svgString = _qrCodeService.GenerateSvg(tableUrl);
        var svgBytes = System.Text.Encoding.UTF8.GetBytes(svgString);
        return new QrResult("image/svg+xml", svgBytes);
    }
}
