namespace RestoCore.Application.Features.Tenants.Commands;

using System.Text.Json.Serialization;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Interfaces.Repositories;
using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Features.Tenants.DTOs;

public record PatchTenantProfileCommand(
    string? Name,
    [property: JsonPropertyName("phone_number")] string? PhoneNumber,
    string? Address,
    [property: JsonPropertyName("business_hours")] string? BusinessHours
) : IRequest<TenantProfileResponse>;

public class PatchTenantProfileCommandHandler : IRequestHandler<PatchTenantProfileCommand, TenantProfileResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICdnPurgeService? _cdnPurgeService;

    public PatchTenantProfileCommandHandler(
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

    public async Task<TenantProfileResponse> Handle(PatchTenantProfileCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        var tenant = await _tenantRepository.GetTenantByIdAsync(tenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant {tenantId} was not found.");

        if (request.Name != null)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Restaurant commercial name cannot be empty.", nameof(request.Name));
            }
            tenant.Name = request.Name.Trim();
        }

        if (request.PhoneNumber != null)
        {
            tenant.PhoneNumber = request.PhoneNumber.Trim();
        }

        if (request.Address != null)
        {
            tenant.Address = request.Address.Trim();
        }

        if (request.BusinessHours != null)
        {
            tenant.BusinessHours = request.BusinessHours.Trim();
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        if (_cdnPurgeService != null && !string.IsNullOrWhiteSpace(tenant.Slug))
        {
            await _cdnPurgeService.PurgeTenantCacheAsync(tenant.Slug, cancellationToken);
        }

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
