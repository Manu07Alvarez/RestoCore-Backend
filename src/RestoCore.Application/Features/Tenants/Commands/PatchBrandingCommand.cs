namespace RestoCore.Application.Features.Tenants.Commands;

using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Common.Mediator;
using RestoCore.Domain.ValueObjects;

public record PatchBrandingCommand(
    string? PrimaryColorRgb = null,
    string? PrimaryColor = null,
    string? SecondaryColor = null,
    string? BackgroundColor = null,
    string? TextColor = null,
    string? LogoUrl = null,
    string? CoverBannerUrl = null,
    string? FontFamily = null,
    string? LayoutMode = null
) : IRequest<BrandingConfig>;

public class PatchBrandingCommandHandler : IRequestHandler<PatchBrandingCommand, BrandingConfig>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICdnPurgeService? _cdnPurgeService;

    public PatchBrandingCommandHandler(
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

    public async Task<BrandingConfig> Handle(PatchBrandingCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var tenant = await _tenantRepository.GetTenantByIdAsync(tenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant {tenantId} was not found.");

        if (!string.IsNullOrWhiteSpace(request.PrimaryColorRgb))
        {
            tenant.BrandingConfig.ApplyRgbColor(request.PrimaryColorRgb);
        }
        else if (!string.IsNullOrWhiteSpace(request.PrimaryColor))
        {
            tenant.BrandingConfig.ApplyRgbColor(request.PrimaryColor);
        }

        if (request.SecondaryColor != null)
        {
            tenant.BrandingConfig.SecondaryColor = request.SecondaryColor.Trim();
        }

        if (request.BackgroundColor != null)
        {
            tenant.BrandingConfig.BackgroundColor = request.BackgroundColor.Trim();
        }

        if (request.TextColor != null)
        {
            tenant.BrandingConfig.TextColor = request.TextColor.Trim();
        }

        if (request.LogoUrl != null)
        {
            tenant.BrandingConfig.LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl)
                ? null
                : request.LogoUrl.Trim();
        }

        if (request.CoverBannerUrl != null)
        {
            tenant.BrandingConfig.CoverBannerUrl = string.IsNullOrWhiteSpace(request.CoverBannerUrl)
                ? null
                : request.CoverBannerUrl.Trim();
        }

        if (request.FontFamily != null)
        {
            tenant.BrandingConfig.FontFamily = request.FontFamily.Trim();
        }

        if (request.LayoutMode != null)
        {
            tenant.BrandingConfig.LayoutMode = request.LayoutMode.Trim();
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        if (_cdnPurgeService != null && !string.IsNullOrWhiteSpace(tenant.Slug))
        {
            await _cdnPurgeService.PurgeTenantCacheAsync(tenant.Slug, cancellationToken);
        }

        return tenant.BrandingConfig;
    }
}
