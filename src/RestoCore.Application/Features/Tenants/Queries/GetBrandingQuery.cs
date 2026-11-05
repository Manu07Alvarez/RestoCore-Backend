namespace RestoCore.Application.Features.Tenants.Queries;

using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Common.Mediator;
using RestoCore.Domain.ValueObjects;

public record GetBrandingQuery() : IRequest<BrandingConfig>;

public class GetBrandingQueryHandler : IRequestHandler<GetBrandingQuery, BrandingConfig>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantContext _tenantContext;

    public GetBrandingQueryHandler(
        ITenantRepository tenantRepository,
        ITenantContext tenantContext)
    {
        _tenantRepository = tenantRepository;
        _tenantContext = tenantContext;
    }

    public async Task<BrandingConfig> Handle(GetBrandingQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var tenant = await _tenantRepository.GetTenantByIdAsync(tenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant {tenantId} was not found.");

        return tenant.BrandingConfig;
    }
}
