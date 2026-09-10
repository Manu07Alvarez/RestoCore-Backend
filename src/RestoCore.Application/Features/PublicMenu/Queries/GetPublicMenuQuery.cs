namespace RestoCore.Application.Features.PublicMenu.Queries;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Features.PublicMenu.DTOs;

public record GetPublicMenuQuery(string TenantSlug, string? TableToken = null) : IRequest<(PublicMenuResponse Response, string ETag)>;

public class GetPublicMenuQueryHandler : IRequestHandler<GetPublicMenuQuery, (PublicMenuResponse Response, string ETag)>
{
    private readonly IApplicationDbContext _context;

    public GetPublicMenuQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(PublicMenuResponse Response, string ETag)> Handle(GetPublicMenuQuery request, CancellationToken cancellationToken)
    {
        // Public lookup ignores tenant query filter since client is unauthenticated
        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(t => t.Categories.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder))
                .ThenInclude(c => c.Items.OrderBy(m => m.DisplayOrder))
                    .ThenInclude(m => m.ModifierGroups)
                        .ThenInclude(mg => mg.Options)
            .FirstOrDefaultAsync(t => t.Slug == request.TenantSlug && t.Status == "Active", cancellationToken);

        if (tenant == null)
        {
            throw new KeyNotFoundException($"Restaurante con slug '{request.TenantSlug}' no fue encontrado o está inactivo.");
        }

        int? tableNumber = null;
        if (!string.IsNullOrEmpty(request.TableToken))
        {
            var table = await _context.Tables
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.TenantId == tenant.Id && t.Token == request.TableToken && t.IsActive, cancellationToken);
            tableNumber = table?.TableNumber;
        }

        var response = new PublicMenuResponse
        {
            TenantName = tenant.Name,
            TenantSlug = tenant.Slug,
            TableNumber = tableNumber,
            Branding = new PublicBrandingDto
            {
                PrimaryColor = tenant.BrandingConfig.PrimaryColor,
                SecondaryColor = tenant.BrandingConfig.SecondaryColor,
                BackgroundColor = tenant.BrandingConfig.BackgroundColor,
                TextColor = tenant.BrandingConfig.TextColor,
                LogoUrl = tenant.BrandingConfig.LogoUrl,
                CoverBannerUrl = tenant.BrandingConfig.CoverBannerUrl,
                FontFamily = tenant.BrandingConfig.FontFamily,
                LayoutMode = tenant.BrandingConfig.LayoutMode
            },
            Categories = tenant.Categories.Select(c => new PublicCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Items = c.Items.Select(m => new PublicMenuItemDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    BasePrice = m.BasePrice,
                    IsAvailable = m.IsAvailable,
                    ImageUrl = m.ImageUrl,
                    Allergens = m.Allergens,
                    DietaryLabels = m.DietaryLabels,
                    ModifierGroups = m.ModifierGroups.Select(mg => new PublicModifierGroupDto
                    {
                        Id = mg.Id,
                        Name = mg.Name,
                        MinSelection = mg.MinSelection,
                        MaxSelection = mg.MaxSelection,
                        IsRequired = mg.IsRequired,
                        Options = mg.Options.Select(mo => new PublicModifierOptionDto
                        {
                            Id = mo.Id,
                            Name = mo.Name,
                            PriceDelta = mo.PriceDelta,
                            IsAvailable = mo.IsAvailable
                        }).ToList()
                    }).ToList()
                }).ToList()
            }).ToList()
        };

        // Compute deterministic ETag from payload JSON
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(response);
        var hash = SHA256.HashData(jsonBytes);
        var etag = $"\"{Convert.ToHexString(hash)[..16].ToLowerInvariant()}\"";

        return (response, etag);
    }
}
