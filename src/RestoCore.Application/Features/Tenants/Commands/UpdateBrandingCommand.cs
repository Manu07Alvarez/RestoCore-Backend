namespace RestoCore.Application.Features.Tenants.Commands;

using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
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
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateBrandingCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<BrandingConfig> Handle(UpdateBrandingCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var tenant = await _tenantRepository.GetTenantByIdAsync(tenantId, cancellationToken);
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

        await _unitOfWork.CommitAsync(cancellationToken);

        return tenant.BrandingConfig;
    }
}
