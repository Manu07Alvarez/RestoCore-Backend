namespace RestoCore.Application.Features.Tenants.Commands;

using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Domain.Entities;
using RestoCore.Domain.ValueObjects;

public record ProvisionTenantCommand(
    string Name,
    string Slug,
    string AdminEmail,
    string? CustomDomain
) : IRequest<TenantSummaryResponse>;

public record TenantSummaryResponse(
    Guid Id,
    string Name,
    string Slug,
    string Status,
    string? CustomDomain
);

public class ProvisionTenantCommandHandler : IRequestHandler<ProvisionTenantCommand, TenantSummaryResponse>
{
    private readonly IApplicationDbContext _context;

    public ProvisionTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TenantSummaryResponse> Handle(ProvisionTenantCommand request, CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();

        var slugExists = await _context.Tenants
            .AnyAsync(t => t.Slug == normalizedSlug, cancellationToken);

        if (slugExists)
        {
            throw new InvalidOperationException($"Tenant slug '{normalizedSlug}' is already registered.");
        }

        if (!string.IsNullOrWhiteSpace(request.CustomDomain))
        {
            var domainExists = await _context.Tenants
                .AnyAsync(t => t.CustomDomain == request.CustomDomain.Trim().ToLowerInvariant(), cancellationToken);

            if (domainExists)
            {
                throw new InvalidOperationException($"Custom domain '{request.CustomDomain}' is already in use.");
            }
        }

        var tenant = new Tenant
        {
            Name = request.Name.Trim(),
            Slug = normalizedSlug,
            CustomDomain = string.IsNullOrWhiteSpace(request.CustomDomain) ? null : request.CustomDomain.Trim().ToLowerInvariant(),
            Status = "Active",
            BrandingConfig = new BrandingConfig()
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);

        return new TenantSummaryResponse(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.Status,
            tenant.CustomDomain
        );
    }
}
