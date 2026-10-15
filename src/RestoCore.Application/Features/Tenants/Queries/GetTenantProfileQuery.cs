namespace RestoCore.Application.Features.Tenants.Queries;

using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Features.Tenants.DTOs;

public record GetTenantProfileQuery() : IRequest<TenantProfileResponse>;

public class GetTenantProfileQueryHandler : IRequestHandler<GetTenantProfileQuery, TenantProfileResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantContext _tenantContext;

    public GetTenantProfileQueryHandler(
        ITenantRepository tenantRepository,
        ITenantContext tenantContext)
    {
        _tenantRepository = tenantRepository;
        _tenantContext = tenantContext;
    }

    public async Task<TenantProfileResponse> Handle(GetTenantProfileQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var tenant = await _tenantRepository.GetTenantByIdAsync(tenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant {tenantId} was not found.");

        return new TenantProfileResponse(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.PhoneNumber,
            tenant.Address,
            tenant.BusinessHours
        );
    }
}
