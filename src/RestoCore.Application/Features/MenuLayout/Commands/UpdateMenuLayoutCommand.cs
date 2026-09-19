namespace RestoCore.Application.Features.MenuLayout.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Features.MenuLayout.DTOs;
using RestoCore.Domain.ValueObjects;

public record UpdateMenuLayoutCommand(LayoutConfigDto Layout) : IRequest<LayoutConfigDto>;

public class UpdateMenuLayoutCommandHandler : IRequestHandler<UpdateMenuLayoutCommand, LayoutConfigDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache? _cache;

    public UpdateMenuLayoutCommandHandler(
        IApplicationDbContext context,
        ITenantContext tenantContext,
        Microsoft.Extensions.Caching.Distributed.IDistributedCache? cache = null)
    {
        _context = context;
        _tenantContext = tenantContext;
        _cache = cache;
    }

    public async Task<LayoutConfigDto> Handle(UpdateMenuLayoutCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant ID is required.");
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
        if (tenant == null)
        {
            throw new KeyNotFoundException($"Tenant {tenantId} was not found.");
        }

        tenant.LayoutConfig = new LayoutConfig
        {
            CanvasEnabled = request.Layout.CanvasEnabled,
            BackgroundUrl = request.Layout.BackgroundUrl,
            BackgroundColor = request.Layout.BackgroundColor,
            Elements = request.Layout.Elements.Select(e => new CanvasElement
            {
                DishId = e.DishId,
                X = e.X,
                Y = e.Y,
                ZIndex = e.ZIndex,
                Width = e.Width,
                Height = e.Height
            }).ToList()
        };

        tenant.UpdatedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        if (_cache != null)
        {
            try
            {
                var versionCacheKey = $"menu_version:{tenant.Slug}";
                var newVersion = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                await _cache.SetStringAsync(versionCacheKey, newVersion, cancellationToken);
            }
            catch
            {
                // Non-blocking cache invalidation failure
            }
        }

        return request.Layout;
    }
}