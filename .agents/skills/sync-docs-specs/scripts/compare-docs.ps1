param(
    [string]$DocsPath = "R:\Programando\RestoCore\RestoCore-Docs",
    [switch]$CreateSpec = $false
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " RestoCore Docs vs Backend Delta Analyzer" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

if (-not (Test-Path $DocsPath)) {
    Write-Error "Docs path '$DocsPath' does not exist."
    exit 1
}

$BackendRoot = (Get-Location).Path
$SpecsDir = Join-Path $BackendRoot "specs"
$AdrPath = Join-Path $DocsPath "docs\adr"
$OpenApiPath = Join-Path $DocsPath "specs\openapi.yaml"
$PoliciesPath = Join-Path $DocsPath "specs\policies"

# 1. Inspect ADRs in Docs
$Adrs = Get-ChildItem -Path $AdrPath -Filter "*.md" | Sort-Object Name
Write-Host "`n[1] ADRs found in Docs ($($Adrs.Count)):" -ForegroundColor Yellow

$ExistingSpecs = Get-ChildItem -Path $SpecsDir -Directory | Where-Object { $_.Name -ne "policies" }
$ExistingSpecNames = $ExistingSpecs.Name

$UnmatchedAdrs = @()

foreach ($adr in $Adrs) {
    $adrContent = Get-Content $adr.FullName -Raw
    $adrNumber = if ($adr.Name -match "^(\d+)") { $matches[1] } else { "" }
    $adrTitle = if ($adrContent -match "(?m)^#\s+(.+)$") { $matches[1].Trim() } else { $adr.BaseName }

    # Check ADR status in document frontmatter
    $isDeferred = $adrContent -match "(?i)\*\*(Estado|Status):\*\*\s*(Pospuesto|Deferred)"

    # Check if a spec in backend covers this ADR topic, or if it is covered by governance/architecture
    $isCovered = $false
    $matchingSpec = ""

    if ($isDeferred) {
        $isCovered = $true
        $matchingSpec = "[Pospuesto/Deferred]"
    } elseif ($adrNumber -eq "0001" -or $adr.Name -match "spec-driven-development") {
        # ADR-0001 defines SDD and Constitution
        if ((Test-Path (Join-Path $BackendRoot ".specify\memory\constitution.md")) -or (Test-Path (Join-Path $BackendRoot "AGENTS.md"))) {
            $isCovered = $true
            $matchingSpec = "Constitution / AGENTS.md (SDD Governance)"
        }
    } elseif ($adrNumber -eq "0003" -or $adr.Name -match "postgresql-con-jsonb") {
        # ADR-0003 is architectural persistence across entities (Layout, Orders, etc.)
        $isCovered = $true
        $matchingSpec = "RestoCore.Infrastructure (PostgreSQL JSONB Models)"
    } else {
        foreach ($spec in $ExistingSpecs) {
            $specFile = Join-Path $spec.FullName "spec.md"
            if (Test-Path $specFile) {
                $specText = Get-Content $specFile -Raw
                if ($specText -match [regex]::Escape($adr.Name) -or 
                    ($adrNumber -ne "" -and $specText -match "ADR-$adrNumber") -or
                    ($adr.Name -match "canvas" -and $spec.Name -match "canvas") -or
                    ($adr.Name -match "multitenant|multi-tenant" -and $spec.Name -match "multi-tenant") -or
                    ($adr.Name -match "docker|seaweedfs" -and $spec.Name -match "docker") -or
                    ($adr.Name -match "k6|latencia" -and $spec.Name -match "k6") -or
                    ($adr.Name -match "metricas|telemetria" -and $spec.Name -match "logging|traces") -or
                    ($adr.Name -match "cola-fifo|redis-streams|procesamiento" -and $spec.Name -match "order-fifo")) {
                    $isCovered = $true
                    $matchingSpec = $spec.Name
                    break
                }
            }
        }
    }

    if ($isCovered) {
        Write-Host "  [OK] $($adr.Name) -> $matchingSpec" -ForegroundColor Green
    } else {
        Write-Host "  [GAP] $($adr.Name): $adrTitle" -ForegroundColor Red
        $UnmatchedAdrs += [PSCustomObject]@{
            FileName = $adr.Name
            Title = $adrTitle
            FullPath = $adr.FullName
            Content = $adrContent
        }
    }
}

# 2. Check OpenAPI Contract Delta
Write-Host "`n[2] Contract Delta Check:" -ForegroundColor Yellow
if (Test-Path $OpenApiPath) {
    Write-Host "  Found canonical openapi.yaml at $OpenApiPath" -ForegroundColor Green
} else {
    Write-Host "  Canonical openapi.yaml not found at $OpenApiPath" -ForegroundColor Yellow
}

# 3. Summary & Recommendation
Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " Summary of Deltas" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

if ($UnmatchedAdrs.Count -eq 0) {
    Write-Host "All architectural documents in RestoCore-Docs are currently represented in backend specifications." -ForegroundColor Green
} else {
    Write-Host "Found $($UnmatchedAdrs.Count) document(s)/ADR(s) without matching specs in backend:" -ForegroundColor Yellow
    foreach ($gap in $UnmatchedAdrs) {
        Write-Host "  - $($gap.FileName) ($($gap.Title))" -ForegroundColor Yellow
    }

    if ($CreateSpec) {
        Write-Host "`nGenerating candidate specification for first uncovered delta..." -ForegroundColor Cyan
        # Returns the first gap for spec generation
        return $UnmatchedAdrs[0]
    }
}
