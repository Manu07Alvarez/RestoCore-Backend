namespace RestoCore.Application.Features.Tenants.Commands;

using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Domain.ValueObjects;

public record UpdateBrandingCommand(
    string? PrimaryColor,
    string? SecondaryColor,
    string? BackgroundColor,
    string? TextColor,
    string? LogoUrl,
    string? CoverBannerUrl,
    string? FontFamily,
    string? LayoutMode,
    string? PrimaryColorRgb = null
) : IRequest<BrandingConfig>;

public class UpdateBrandingCommandHandler : IRequestHandler<UpdateBrandingCommand, BrandingConfig>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICdnPurgeService? _cdnPurgeService;

    public UpdateBrandingCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ICdnPurgeService? cdnPurgeService = null)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _cdnPurgeService = cdnPurgeService;
    }

    public async Task<BrandingConfig> Handle(UpdateBrandingCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var tenant = await _tenantRepository.GetTenantByIdAsync(tenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant {tenantId} was not found.");

        var branding = new BrandingConfig
        {
            SecondaryColor = string.IsNullOrWhiteSpace(request.SecondaryColor) ? "#1D3557" : request.SecondaryColor.Trim(),
            BackgroundColor = string.IsNullOrWhiteSpace(request.BackgroundColor) ? "#F8F9FA" : request.BackgroundColor.Trim(),
            TextColor = string.IsNullOrWhiteSpace(request.TextColor) ? "#2B2D42" : request.TextColor.Trim(),
            LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim(),
            CoverBannerUrl = string.IsNullOrWhiteSpace(request.CoverBannerUrl) ? null : request.CoverBannerUrl.Trim(),
            FontFamily = string.IsNullOrWhiteSpace(request.FontFamily) ? "Inter, sans-serif" : request.FontFamily.Trim(),
            LayoutMode = string.IsNullOrWhiteSpace(request.LayoutMode) ? "GridWithImages" : request.LayoutMode.Trim()
        };

        if (!string.IsNullOrWhiteSpace(request.PrimaryColorRgb))
        {
            branding.ApplyRgbColor(request.PrimaryColorRgb);
        }
        else if (!string.IsNullOrWhiteSpace(request.PrimaryColor))
        {
            branding.ApplyRgbColor(request.PrimaryColor);
        }

        tenant.BrandingConfig = branding;

        await _unitOfWork.CommitAsync(cancellationToken);

        if (_cdnPurgeService != null && !string.IsNullOrWhiteSpace(tenant.Slug))
        {
            await _cdnPurgeService.PurgeTenantCacheAsync(tenant.Slug, cancellationToken);
        }

        return tenant.BrandingConfig;
    }
}
