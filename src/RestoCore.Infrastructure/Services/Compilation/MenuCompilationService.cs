namespace RestoCore.Infrastructure.Services.Compilation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Features.MenuLayout.DTOs;
using RestoCore.Application.Features.MenuLayout.Services;

public class MenuCompilationService : IMenuCompilationService
{
    private readonly IApplicationDbContext _context;

    public MenuCompilationService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MenuCompilationResult> CompileAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .Include(t => t.Categories.Where(c => c.IsActive))
                .ThenInclude(c => c.Items)
            .FirstOrDefaultAsync(t => t.Id == tenantId, ct);

        if (tenant == null)
        {
            throw new KeyNotFoundException($"Tenant con ID '{tenantId}' no fue encontrado.");
        }

        var activeDishIds = tenant.Categories
            .Where(c => c.IsActive)
            .SelectMany(c => c.Items)
            .Where(i => i.IsAvailable)
            .Select(i => i.Id)
            .ToHashSet();

        LayoutConfigDto? compiledLayout = null;
        int assetsQueued = 0;

        if (tenant.LayoutConfig != null)
        {
            if (!string.IsNullOrWhiteSpace(tenant.LayoutConfig.BackgroundUrl))
            {
                assetsQueued++;
            }

            var prunedElements = tenant.LayoutConfig.Elements
                .Where(e => activeDishIds.Contains(e.DishId))
                .OrderBy(e => e.ZIndex)
                .ThenBy(e => e.Y)
                .ThenBy(e => e.X)
                .Select(e => new CanvasElementDto
                {
                    DishId = e.DishId,
                    X = e.X,
                    Y = e.Y,
                    ZIndex = e.ZIndex,
                    Width = e.Width,
                    Height = e.Height
                }).ToList();

            compiledLayout = new LayoutConfigDto
            {
                CanvasEnabled = tenant.LayoutConfig.CanvasEnabled,
                BackgroundUrl = tenant.LayoutConfig.BackgroundUrl,
                BackgroundColor = tenant.LayoutConfig.BackgroundColor,
                Elements = prunedElements
            };
        }

        // Add media assets from active menu items
        assetsQueued += tenant.Categories
            .SelectMany(c => c.Items)
            .Count(i => i.IsAvailable && !string.IsNullOrWhiteSpace(i.ImageUrl));

        var now = DateTime.UtcNow;
        var versionHash = ComputeVersionHash(compiledLayout, activeDishIds, now);

        tenant.CurrentVersionHash = versionHash;
        await _context.SaveChangesAsync(ct);

        return new MenuCompilationResult
        {
            VersionHash = versionHash,
            CompiledLayout = compiledLayout,
            AssetsQueued = assetsQueued
        };
    }

    public string ComputeVersionHash(LayoutConfigDto? layout, IEnumerable<Guid> activeDishIds, DateTime timestamp)
    {
        var snapshot = new
        {
            Layout = layout,
            ActiveDishes = activeDishIds.OrderBy(id => id).ToList()
        };

        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(snapshot);
        var hashBytes = SHA256.HashData(jsonBytes);
        var hashHex = Convert.ToHexString(hashBytes)[..8].ToLowerInvariant();
        return $"v{timestamp:yyyyMMddHHmmss}-{hashHex}";
    }
}
