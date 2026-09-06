namespace RestoCore.Application.Features.Tenants.Commands;

using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Domain.ValueObjects;

public record UpdateBrandingCommand(
    string PrimaryColor,
    string? SecondaryColor,
    string? BackgroundColor,
    string? TextColor,
    string? LogoUrl,
    string? CoverBannerUrl,
    string FontFamily,
    string LayoutMode
) : IRequest<BrandingConfig>;

public class UpdateBrandingCommandHandler : IRequestHandler<UpdateBrandingCommand, BrandingConfig>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public UpdateBrandingCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<BrandingConfig> Handle(UpdateBrandingCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant == null)
        {
            throw new KeyNotFoundException($"Tenant {tenantId} was not found.");
        }

        tenant.BrandingConfig = new BrandingConfig
        {
            PrimaryColor = request.PrimaryColor,
            SecondaryColor = request.SecondaryColor ?? "#1D3557",
            BackgroundColor = request.BackgroundColor ?? "#F8F9FA",
            TextColor = request.TextColor ?? "#2B2D42",
            LogoUrl = request.LogoUrl,
            CoverBannerUrl = request.CoverBannerUrl,
            FontFamily = request.FontFamily,
            LayoutMode = request.LayoutMode
        };

        await _context.SaveChangesAsync(cancellationToken);

        return tenant.BrandingConfig;
    }
}
