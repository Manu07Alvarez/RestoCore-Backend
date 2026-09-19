namespace RestoCore.Application.Features.MenuLayout.Commands;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using RestoCore.Application.Common.Interfaces;
using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Features.MenuLayout.DTOs;
using RestoCore.Application.Features.MenuLayout.Services;
using RestoCore.Domain.Entities;

public record PublishMenuCommand(MenuPublishRequest? Request = null) : IRequest<MenuPublishJobResponse>;

public class PublishMenuCommandHandler : IRequestHandler<PublishMenuCommand, MenuPublishJobResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly IMenuCompilationService _compilationService;
    private readonly ICdnPurgeService _cdnPurgeService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<PublishMenuCommandHandler> _logger;

    public PublishMenuCommandHandler(
        IApplicationDbContext context,
        ITenantContext tenantContext,
        IMenuCompilationService compilationService,
        ICdnPurgeService cdnPurgeService,
        IDistributedCache cache,
        ILogger<PublishMenuCommandHandler> logger)
    {
        _context = context;
        _tenantContext = tenantContext;
        _compilationService = compilationService;
        _cdnPurgeService = cdnPurgeService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<MenuPublishJobResponse> Handle(PublishMenuCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant ID is required.");
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
        if (tenant == null)
        {
            throw new KeyNotFoundException($"Tenant '{tenantId}' was not found.");
        }

        var job = new MenuPublishJob
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            TenantSlug = tenant.Slug,
            Status = "processing",
            TriggeredAt = DateTimeOffset.UtcNow,
            EstimatedDurationSeconds = 3,
            VersionHash = string.Empty
        };

        _context.MenuPublishJobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            // 1. Run AOT compilation
            var compilationResult = await _compilationService.CompileAsync(tenant.Id, cancellationToken);
            job.VersionHash = compilationResult.VersionHash;
            job.AssetsQueued = compilationResult.AssetsQueued;

            // 2. Selective CDN cache purge
            var purgeTags = new[] { $"tenant:{tenant.Slug}", $"menu:{tenant.Slug}" };
            await _cdnPurgeService.PurgeTagsAsync(purgeTags, cancellationToken);
            job.CdnPurgeRequested = true;

            // 3. Synchronize distributed Redis cache invalidation
            try
            {
                var versionCacheKey = $"menu_version:{tenant.Slug}";
                var newVersion = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                await _cache.SetStringAsync(versionCacheKey, newVersion, cancellationToken);
            }
            catch (Exception cacheEx)
            {
                _logger.LogWarning(cacheEx, "Failed to invalidate Redis menu version cache for tenant {Slug}", tenant.Slug);
            }

            job.Status = "completed";
            job.CompletedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AOT compilation pipeline failed for tenant {Slug}", tenant.Slug);
            job.Status = "failed";
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            throw;
        }

        return new MenuPublishJobResponse
        {
            JobId = job.Id,
            TenantSlug = job.TenantSlug,
            Status = job.Status,
            EstimatedDurationSeconds = job.EstimatedDurationSeconds,
            VersionHash = job.VersionHash,
            CdnPurgeRequested = job.CdnPurgeRequested,
            AssetsQueued = job.AssetsQueued,
            TriggeredAt = job.TriggeredAt.UtcDateTime
        };
    }
}
