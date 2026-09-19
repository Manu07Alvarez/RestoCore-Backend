namespace RestoCore.Application.Features.MenuLayout.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestoCore.Application.Features.MenuLayout.DTOs;

public class MenuCompilationResult
{
    public string VersionHash { get; set; } = string.Empty;
    public LayoutConfigDto? CompiledLayout { get; set; }
    public int AssetsQueued { get; set; }
}

public interface IMenuCompilationService
{
    Task<MenuCompilationResult> CompileAsync(Guid tenantId, CancellationToken ct = default);
    string ComputeVersionHash(LayoutConfigDto? layout, IEnumerable<Guid> activeDishIds, DateTime timestamp);
}
