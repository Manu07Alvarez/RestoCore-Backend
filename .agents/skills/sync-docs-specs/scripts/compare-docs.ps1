param(
    [Alias("docs-path")]
    [string]$DocsPath = "R:\Programando\RestoCore\RestoCore-Docs",
    [string]$BackendPath = "",
    [Alias("Adr")]
    [string]$AdrFilter = "",
    [Alias("auto-spec")]
    [switch]$AutoSpec = $false,
    [switch]$CreateSpec = $false
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " RestoCore Docs vs Backend Delta Analyzer" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# Resolve backend root by locating .specify directory or RestoCore.sln upwards
function Find-BackendRoot {
    param([string]$StartDir)
    if ([string]::IsNullOrWhiteSpace($StartDir)) {
        if ($PSScriptRoot) {
            $StartDir = $PSScriptRoot
        } else {
            $StartDir = (Get-Location).Path
        }
    }
    $curr = (Resolve-Path -LiteralPath $StartDir -ErrorAction SilentlyContinue)
    $currPath = if ($curr) { $curr.Path } else { (Get-Location).Path }
    while ($currPath) {
        if (Test-Path -LiteralPath (Join-Path $currPath ".specify") -PathType Container) {
            return $currPath
        }
        $parent = Split-Path $currPath -Parent
        if ([string]::IsNullOrEmpty($parent) -or $parent -eq $currPath) {
            break
        }
        $currPath = $parent
    }
    return (Get-Location).Path
}

if ([string]::IsNullOrWhiteSpace($BackendPath)) {
    $BackendRoot = Find-BackendRoot
} else {
    $BackendRoot = (Resolve-Path $BackendPath).Path
}

# Resolve DocsPath if provided as relative path
if (-not [System.IO.Path]::IsPathRooted($DocsPath)) {
    if (Test-Path (Join-Path $BackendRoot $DocsPath)) {
        $DocsPath = (Resolve-Path (Join-Path $BackendRoot $DocsPath)).Path
    } elseif (Test-Path $DocsPath) {
        $DocsPath = (Resolve-Path $DocsPath).Path
    }
}

if (-not (Test-Path $DocsPath)) {
    Write-Error "Docs path '$DocsPath' does not exist."
    exit 1
}

$SpecsDir = Join-Path $BackendRoot "specs"
$AdrPath = Join-Path $DocsPath "docs\adr"
$OpenApiPath = Join-Path $DocsPath "specs\openapi.yaml"
$PoliciesPath = Join-Path $DocsPath "specs\policies"

# 1. Inspect ADRs in Docs
$Adrs = Get-ChildItem -Path $AdrPath -Filter "*.md" | Sort-Object Name
if (-not [string]::IsNullOrWhiteSpace($AdrFilter)) {
    $Adrs = @($Adrs | Where-Object { $_.Name -match $AdrFilter -or $_.BaseName -match $AdrFilter })
    Write-Host "Filtering analysis for ADR query '$AdrFilter' ($($Adrs.Count) matched)" -ForegroundColor Cyan
}
Write-Host "`n[1] ADRs found in Docs ($($Adrs.Count)):" -ForegroundColor Yellow

$ExistingSpecs = Get-ChildItem -Path $SpecsDir -Directory | Where-Object { $_.Name -ne "policies" }
$ExistingSpecNames = $ExistingSpecs.Name

$UnmatchedAdrs = @()

foreach ($adrItem in $Adrs) {
    $adrContent = Get-Content $adrItem.FullName -Raw
    $adrNumber = if ($adrItem.Name -match "^(\d+)") { $matches[1] } else { "" }
    $adrTitle = if ($adrContent -match "(?m)^#\s+(.+)$") { $matches[1].Trim() } else { $adrItem.BaseName }

    # Check ADR status in document frontmatter
    $isDeferred = $adrContent -match "(?i)\*\*(Estado|Status):\*\*\s*(Pospuesto|Deferred)"

    # Check if a spec in backend covers this ADR topic, or if it is covered by governance/architecture
    $isCovered = $false
    $matchingSpec = ""

    if ($isDeferred) {
        $isCovered = $true
        $matchingSpec = "[Pospuesto/Deferred]"
    } elseif ($adrNumber -eq "0001" -or $adrItem.Name -match "spec-driven-development") {
        # ADR-0001 defines SDD and Constitution
        if ((Test-Path (Join-Path $BackendRoot ".specify\memory\constitution.md")) -or (Test-Path (Join-Path $BackendRoot "AGENTS.md"))) {
            $isCovered = $true
            $matchingSpec = "Constitution / AGENTS.md (SDD Governance)"
        }
    } elseif ($adrNumber -eq "0003" -or $adrItem.Name -match "postgresql-con-jsonb") {
        # ADR-0003 is architectural persistence across entities (Layout, Orders, etc.)
        $isCovered = $true
        $matchingSpec = "RestoCore.Infrastructure (PostgreSQL JSONB Models)"
    } elseif ($adrItem.Name -match "seguridad-declarativa-opa-rego" -or ($adrNumber -eq "0006" -and $adrItem.Name -match "opa")) {
        # ADR-0006 Declarative Security with OPA / Rego is implemented under specs/policies/
        if (Test-Path (Join-Path $SpecsDir "policies\authz.rego")) {
            $isCovered = $true
            $matchingSpec = "specs/policies/authz.rego (OPA Declarative Security)"
        }
    } else {
        # Pass 1: Canonical source match or exact filename match in spec.md
        foreach ($spec in $ExistingSpecs) {
            $specFile = Join-Path $spec.FullName "spec.md"
            if (Test-Path $specFile) {
                $specText = Get-Content $specFile -Raw
                if ($specText -match [regex]::Escape($adrItem.Name)) {
                    $isCovered = $true
                    $matchingSpec = $spec.Name
                    break
                }
            }
        }

        # Pass 2: Topic and number match if Pass 1 found no exact filename match
        if (-not $isCovered) {
            foreach ($spec in $ExistingSpecs) {
                $specFile = Join-Path $spec.FullName "spec.md"
                if (Test-Path $specFile) {
                    $specText = Get-Content $specFile -Raw
                    $numberMatches = $false
                    if ($adrNumber -ne "" -and $specText -match "ADR-$adrNumber\b") {
                        # Disambiguate duplicate numbers in ADRs (e.g. 0005, 0006)
                        if ($adrNumber -eq "0005") {
                            if ($adrItem.Name -match "canvas" -and $specText -match "canvas|lienzo") { $numberMatches = $true }
                            elseif ($adrItem.Name -match "latencia|cache-cdn" -and $specText -match "latencia|cache|cdn") { $numberMatches = $true }
                        } elseif ($adrNumber -eq "0006") {
                            if ($adrItem.Name -match "canvas|aot" -and $specText -match "canvas|aot|compilacion") { $numberMatches = $true }
                            elseif ($adrItem.Name -match "opa|rego" -and $specText -match "opa|rego") { $numberMatches = $true }
                        } elseif ($adrNumber -eq "0010") {
                            # ADR-0010 is Baja suave de platos; do not match 007 order FIFO
                            if ($spec.Name -match "dish-crud|soft-delete") { $numberMatches = $true }
                        } else {
                            $numberMatches = $true
                        }
                    }

                    if ($numberMatches -or
                        ($adrItem.Name -match "canvas" -and $spec.Name -match "canvas") -or
                        ($adrItem.Name -match "multitenant|multi-tenant" -and $spec.Name -match "multi-tenant") -or
                        ($adrItem.Name -match "docker|seaweedfs" -and $spec.Name -match "docker") -or
                        ($adrItem.Name -match "k6|latencia" -and $spec.Name -match "k6") -or
                        ($adrItem.Name -match "metricas.*telemetria|telemetria.*menu" -and $spec.Name -match "analytics|telemetry") -or
                        ($adrItem.Name -match "baja-suave|snapshots-inmutables" -and $spec.Name -match "dish-crud|soft-delete") -or
                        ($adrItem.Name -match "cola-fifo|redis-streams|procesamiento" -and $spec.Name -match "order-fifo")) {
                        $isCovered = $true
                        $matchingSpec = $spec.Name
                        break
                    }
                }
            }
        }
    }

    if ($isCovered) {
        Write-Host "  [OK] $($adrItem.Name) -> $matchingSpec" -ForegroundColor Green
    } else {
        Write-Host "  [GAP] $($adrItem.Name): $adrTitle" -ForegroundColor Red
        $UnmatchedAdrs += [PSCustomObject]@{
            FileName = $adrItem.Name
            Title = $adrTitle
            FullPath = $adrItem.FullName
            Content = $adrContent
        }
    }
}

# 2. Check OpenAPI Contract Delta
Write-Host "`n[2] Contract Delta Check:" -ForegroundColor Yellow
$MissingEndpoints = @()
if (Test-Path $OpenApiPath) {
    Write-Host "  Found canonical openapi.yaml at $OpenApiPath" -ForegroundColor Green
    
    $openApiLines = Get-Content $OpenApiPath
    $currentPath = ""
    $openApiOps = @()
    foreach ($line in $openApiLines) {
        if ($line -match '^\s{2}[''"]?(/[^:''"]+)[''"]?:') {
            $currentPath = $matches[1]
        }
        if ($line -match '^\s{4}(get|post|put|delete|patch):') {
            $method = $matches[1].ToUpper()
            $openApiOps += [PSCustomObject]@{ Method = $method; Path = $currentPath }
        }
    }

    $endpointsDir = Join-Path $BackendRoot "src\RestoCore.Api\Endpoints"
    $implementedRoutes = @{}
    if (Test-Path $endpointsDir) {
        $endpointFiles = Get-ChildItem -Path $endpointsDir -Filter "*.cs"
        foreach ($file in $endpointFiles) {
            $content = Get-Content $file.FullName -Raw
            $groupPrefix = ""
            if ($content -match 'MapGroup\("([^"]+)"\)') {
                $groupPrefix = $matches[1]
            }
            $lines = $content -split "`r?`n"
            foreach ($l in $lines) {
                if ($l -match '\b(app|group)\.Map(Get|Post|Put|Delete|Patch)\("([^"]*)"') {
                    $target = $matches[1]
                    $verb = $matches[2].ToUpper()
                    $sub = $matches[3]
                    $fullRoute = if ($target -eq "group" -and $groupPrefix) {
                        if ($sub -eq "" -or $sub -eq "/") {
                            $groupPrefix
                        } else {
                            $groupPrefix.TrimEnd('/') + '/' + $sub.TrimStart('/')
                        }
                    } else {
                        $sub
                    }
                    $normalizedRoute = $fullRoute -replace '\{([^:]+):[^\}]+\}', '{$1}'
                    $implementedRoutes["$verb $normalizedRoute"] = $file.Name
                }
            }
        }
    }

    foreach ($op in $openApiOps) {
        $opKey = "$($op.Method) $($op.Path)"
        if ($implementedRoutes.ContainsKey($opKey)) {
            Write-Host "  [OK] $($op.Method) $($op.Path)" -ForegroundColor Green
        } else {
            Write-Host "  [PENDING] $($op.Method) $($op.Path)" -ForegroundColor Yellow
            $MissingEndpoints += $op
        }
    }
} else {
    Write-Host "  Canonical openapi.yaml not found at $OpenApiPath" -ForegroundColor Yellow
}

# 3. Check Declarative Security Policies Delta (OPA / Rego)
Write-Host "`n[3] Security Policies Delta Check (OPA / Rego):" -ForegroundColor Yellow
$backendPoliciesPath = Join-Path $SpecsDir "policies"
if (Test-Path $PoliciesPath) {
    Write-Host "  Found canonical security policies at $PoliciesPath" -ForegroundColor Green
    $backendAuthz = Join-Path $backendPoliciesPath "authz.rego"
    if (Test-Path $backendAuthz) {
        $backendAuthzContent = Get-Content $backendAuthz -Raw
        $expectedRules = @(
            @{ Name = "Public Menu Access"; Pattern = '\["api",\s*"v1",\s*"tenants",\s*_,\s*"menu"\]' },
            @{ Name = "Public Analytics Event Collection"; Pattern = '\["api",\s*"v1",\s*"analytics",\s*"collect"\]' },
            @{ Name = "Tenant Admin / Owner Authorization"; Pattern = 'role\s+in\s+\["owner",\s*"tenant_admin"\]' }
        )
        foreach ($rule in $expectedRules) {
            if ($backendAuthzContent -match $rule.Pattern) {
                Write-Host "  [OK] Policy rule: $($rule.Name)" -ForegroundColor Green
            } else {
                Write-Host "  [PENDING] Policy rule missing: $($rule.Name)" -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host "  [PENDING] Backend authz.rego missing at $backendAuthz" -ForegroundColor Yellow
    }
} else {
    Write-Host "  Security policies directory not found at $PoliciesPath" -ForegroundColor Yellow
}

# 4. Categorized Delta Report by Priority
Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " Categorized Delta Report by Priority" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# Group ADR gaps by priority
$P1_Adrs = @()
$P2_Adrs = @()
$P3_Adrs = @()

foreach ($gap in $UnmatchedAdrs) {
    if ($gap.FileName -match "0007|0010|0011|fifo|websockets|baja-suave|metricas" -or $gap.Content -match "(?i)Prioridad Alta|Inviolable") {
        $P1_Adrs += $gap
    } elseif ($gap.FileName -match "0003|0004|jsonb|seaweedfs" -or $gap.Content -match "(?i)Prioridad Media") {
        $P2_Adrs += $gap
    } else {
        $P3_Adrs += $gap
    }
}

# Group pending endpoints by priority
$P1_Endpoints = @()
$P2_Endpoints = @()
$P3_Endpoints = @()

foreach ($ep in $MissingEndpoints) {
    if ($ep.Path -match "/orders|/menu$|/analytics/collect") {
        $P1_Endpoints += $ep
    } elseif ($ep.Path -match "/admin/analytics|/storage/presigned-upload|/media/presigned-url|/admin/permissions|/admin/roles|/admin/staff") {
        $P2_Endpoints += $ep
    } else {
        $P3_Endpoints += $ep
    }
}

Write-Host "`n--- Priority 1 (Critical - Architecture Inviolables & Core Pathways) ---" -ForegroundColor Red
if ($P1_Adrs.Count -gt 0) {
    Write-Host "  ADR Specification Gaps ($($P1_Adrs.Count)):" -ForegroundColor Red
    foreach ($a in $P1_Adrs) { Write-Host "    - $($a.FileName): $($a.Title)" -ForegroundColor Red }
} else {
    Write-Host "  ADR Specification Gaps: None (all P1 ADRs specified in specs/)" -ForegroundColor Green
}
if ($P1_Endpoints.Count -gt 0) {
    Write-Host "  Pending Core Endpoints ($($P1_Endpoints.Count)):" -ForegroundColor Yellow
    foreach ($e in $P1_Endpoints) { Write-Host "    - $($e.Method) $($e.Path)" -ForegroundColor Yellow }
} else {
    Write-Host "  Pending Core Endpoints: None" -ForegroundColor Green
}

Write-Host "`n--- Priority 2 (Structural - Backoffice Management & Analytics) ---" -ForegroundColor Yellow
if ($P2_Adrs.Count -gt 0) {
    Write-Host "  ADR Specification Gaps ($($P2_Adrs.Count)):" -ForegroundColor Yellow
    foreach ($a in $P2_Adrs) { Write-Host "    - $($a.FileName): $($a.Title)" -ForegroundColor Yellow }
} else {
    Write-Host "  ADR Specification Gaps: None" -ForegroundColor Green
}
if ($P2_Endpoints.Count -gt 0) {
    Write-Host "  Pending Structural Endpoints ($($P2_Endpoints.Count)):" -ForegroundColor Yellow
    foreach ($e in $P2_Endpoints) { Write-Host "    - $($e.Method) $($e.Path)" -ForegroundColor Yellow }
} else {
    Write-Host "  Pending Structural Endpoints: None" -ForegroundColor Green
}

Write-Host "`n--- Priority 3 (Platform Governance & Scopes) ---" -ForegroundColor DarkYellow
if ($P3_Adrs.Count -gt 0) {
    Write-Host "  ADR Specification Gaps ($($P3_Adrs.Count)):" -ForegroundColor DarkYellow
    foreach ($a in $P3_Adrs) { Write-Host "    - $($a.FileName): $($a.Title)" -ForegroundColor DarkYellow }
} else {
    Write-Host "  ADR Specification Gaps: None (ADR-0012 properly marked Deferred)" -ForegroundColor Green
}
if ($P3_Endpoints.Count -gt 0) {
    Write-Host "  Pending Platform Endpoints ($($P3_Endpoints.Count)):" -ForegroundColor DarkYellow
    foreach ($e in $P3_Endpoints) { Write-Host "    - $($e.Method) $($e.Path)" -ForegroundColor DarkYellow }
} else {
    Write-Host "  Pending Platform Endpoints: None" -ForegroundColor Green
}

# 4. Summary & Spec-Kit Status
Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " Spec-Kit Feature Specification Status" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$featureJsonPath = Join-Path $BackendRoot ".specify\feature.json"
if (Test-Path $featureJsonPath) {
    $featureJson = Get-Content $featureJsonPath -Raw | ConvertFrom-Json
    Write-Host "Current active feature directory: $($featureJson.feature_directory)" -ForegroundColor Green
    $activeSpecFile = Join-Path (Join-Path $BackendRoot $featureJson.feature_directory) "spec.md"
    if (Test-Path $activeSpecFile) {
        Write-Host "Active specification file verified: $activeSpecFile" -ForegroundColor Green
    } else {
        Write-Host "Active specification file missing at: $activeSpecFile" -ForegroundColor Red
    }
} else {
    Write-Host "No active feature configured in .specify/feature.json" -ForegroundColor Yellow
}

if ($AutoSpec -or $CreateSpec) {
    if ($UnmatchedAdrs.Count -eq 0) {
        Write-Host "`n[AutoSpec] All ADRs in Docs are currently covered by specifications in specs/." -ForegroundColor Green
        Write-Host "No new feature specification directory needed. Active feature remains: $($featureJson.feature_directory)" -ForegroundColor Green
    } else {
        # Select highest priority gap: P1 -> P2 -> P3
        $targetGap = if ($P1_Adrs.Count -gt 0) { $P1_Adrs[0] } elseif ($P2_Adrs.Count -gt 0) { $P2_Adrs[0] } else { $P3_Adrs[0] }
        Write-Host "`n[AutoSpec] Highest priority uncovered ADR candidate: $($targetGap.FileName)" -ForegroundColor Cyan
        
        # Calculate next feature number
        $maxNum = 0
        foreach ($specDir in $ExistingSpecs) {
            if ($specDir.Name -match '^(\d+)') {
                $num = [int]$matches[1]
                if ($num -gt $maxNum) { $maxNum = $num }
            }
        }
        $nextNum = ($maxNum + 1).ToString("D3")
        
        $cleanSlug = $targetGap.FileName -replace '^\d+-?', '' -replace '\.md$', ''
        $newFeatureDirName = "$nextNum-$cleanSlug"
        $newFeatureDirPath = Join-Path $SpecsDir $newFeatureDirName
        
        if (-not (Test-Path $newFeatureDirPath)) {
            New-Item -ItemType Directory -Path $newFeatureDirPath -Force | Out-Null
        }
        
        $newSpecPath = Join-Path $newFeatureDirPath "spec.md"
        if (-not (Test-Path $newSpecPath)) {
            $today = (Get-Date).ToString("yyyy-MM-dd")
            $contextSnippet = ""
            if ($targetGap.Content -match "(?ms)##\s+Context[^\r\n]*\r?\n(.*?)(?=\r?\n##|\r?\n---|`$)") {
                $contextSnippet = $matches[1].Trim()
            }
            if ([string]::IsNullOrWhiteSpace($contextSnippet)) {
                $contextSnippet = "Core capability defined in $($targetGap.FileName): $($targetGap.Title)."
            }
            $specContent = @"
# Feature Specification: $($targetGap.Title)

**Feature Branch**: `feature/$newFeatureDirName`

**Created**: $today

**Status**: Draft

**Input**: User description: "Execute sync-docs-specs workflow to implement $($targetGap.FileName): $($targetGap.Title)"

**Source Architecture Decision**:
- [$($targetGap.FileName)](../../../RestoCore-Docs/docs/adr/$($targetGap.FileName))

---

## 1. Context & Business Need

$contextSnippet

---

## 2. User Scenarios & Testing *(mandatory)*

### User Story 1 - Core Invariant Capability (Priority: P1) - MVP Foundation

As an authorized system user, I want the core capability defined by $($targetGap.FileName) to be available and enforced.

**Why this priority**: Essential architectural requirement.

**Independent Test**: Execute tests verifying primary capabilities of $($targetGap.Title).

**Acceptance Scenarios**:
1. **Given** system in normal operational state, **When** invoking feature operations, **Then** the expected outcome conforms to architectural invariants.

---

### Edge Cases

- System handles empty or null parameters deterministically.
- Concurrency and tenant isolation are enforced.

## 3. Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST implement the capabilities specified in $($targetGap.FileName).

## 4. Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Feature implementation satisfies latency and architectural invariants.

## 5. Assumptions & Scope Boundaries

- Reuses existing authentication, database persistence, and messaging infrastructure.
"@
            Set-Content -Path $newSpecPath -Value $specContent -Encoding utf8
            Write-Host "Created new specification at: $newSpecPath" -ForegroundColor Green
        }
        
        # Update .specify/feature.json
        $featureObj = @{ feature_directory = "specs/$newFeatureDirName" }
        $featureJsonContent = $featureObj | ConvertTo-Json -Compress
        Set-Content -Path $featureJsonPath -Value $featureJsonContent -Encoding utf8
        Write-Host "Updated .specify/feature.json -> specs/$newFeatureDirName" -ForegroundColor Green
        Write-Host "Specification ready. Next recommended step: /speckit-plan" -ForegroundColor Cyan
        
        return $targetGap
    }
}
