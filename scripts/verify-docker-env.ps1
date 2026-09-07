<#
.SYNOPSIS
    Automated verification script for RestoCore Docker development and test environment.
.DESCRIPTION
    Validates Docker daemon availability, spins up containerized dependencies (PostgreSQL,
    Redis, SeaweedFS, OPA), ensures container health, applies EF Core database migrations,
    and executes unit, integration, and security test suites.
#>

[CmdletBinding()]
param(
    [switch]$SkipComposeUp,
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host "`n[STEP] $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Failure {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

Write-Step "Checking Docker availability"
try {
    $dockerVersion = docker version --format '{{.Server.Version}}' 2>$null
    if (-not $dockerVersion) {
        throw "Docker engine is not running or accessible."
    }
    Write-Success "Docker engine is running (version $dockerVersion)"
}
catch {
    Write-Failure $_.Exception.Message
    exit 1
}

if (-not $SkipComposeUp) {
    Write-Step "Starting containerized services via docker-compose.dev.yml"
    docker compose -f docker-compose.dev.yml up -d
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Failed to start Docker Compose services."
        exit 1
    }
    Write-Success "Docker containers launched successfully"
}

Write-Step "Waiting for service readiness..."
$timeoutSeconds = 60
$elapsed = 0
$servicesReady = $false

while ($elapsed -lt $timeoutSeconds) {
    try {
        $pgHealthy = (docker inspect --format='{{.State.Health.Status}}' restocore-postgres-dev 2>$null).Trim()
        $redisHealthy = (docker inspect --format='{{.State.Health.Status}}' restocore-redis-dev 2>$null).Trim()
        $seaweedHealthy = (docker inspect --format='{{.State.Health.Status}}' restocore-seaweedfs-dev 2>$null).Trim()
        $opaRunning = (docker inspect --format='{{.State.Status}}' restocore-opa-dev 2>$null).Trim()

        if ($pgHealthy -eq 'healthy' -and $redisHealthy -eq 'healthy' -and $seaweedHealthy -eq 'healthy' -and $opaRunning -eq 'running') {
            $servicesReady = $true
            break
        }
    }
    catch {
        # Retry until timeout
    }
    Start-Sleep -Seconds 2
    $elapsed += 2
}

if (-not $servicesReady) {
    Write-Failure "Timeout reached while waiting for containers to become healthy."
    docker compose -f docker-compose.dev.yml ps
    exit 1
}
Write-Success "All dependencies are healthy (PostgreSQL, Redis, SeaweedFS, OPA)"

Write-Step "Applying database migrations (dotnet ef database update)"
dotnet ef database update --project src/RestoCore.Infrastructure --startup-project src/RestoCore.Api
if ($LASTEXITCODE -ne 0) {
    Write-Failure "Failed to apply Entity Framework Core migrations."
    exit 1
}
Write-Success "Database migrations applied successfully"

if (-not $SkipTests) {
    Write-Step "Running Unit Tests"
    dotnet test tests/RestoCore.UnitTests/RestoCore.UnitTests.csproj --logger "console;verbosity=normal"
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Unit tests failed."
        exit 1
    }

    Write-Step "Running Integration Tests"
    dotnet test tests/RestoCore.IntegrationTests/RestoCore.IntegrationTests.csproj --logger "console;verbosity=normal"
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Integration tests failed."
        exit 1
    }

    Write-Step "Running Security Tests"
    dotnet test tests/RestoCore.SecurityTests/RestoCore.SecurityTests.csproj --logger "console;verbosity=normal"
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Security tests failed."
        exit 1
    }

    Write-Success "All test suites completed successfully with zero failures."
}

Write-Host "`n========================================================" -ForegroundColor Green
Write-Host " Docker Development Environment Verification: PASSED" -ForegroundColor Green
Write-Host "========================================================`n" -ForegroundColor Green
