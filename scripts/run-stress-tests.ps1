<#
.SYNOPSIS
    Automated runner for k6 stress and load tests with Docker fallback.
.DESCRIPTION
    Executes specified k6 test scenario using native k6 CLI if available,
    otherwise executes seamlessly via grafana/k6 Docker container.
#>

[CmdletBinding()]
param(
    [ValidateSet("public-menu-load", "etag-cache-test", "public-menu-stress", "all-endpoints-test")]
    [string]$Scenario = "public-menu-load",
    [string]$BaseUrl = "http://localhost:5221",
    [string]$TenantSlug = "bodegon-0320458"
)

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$K6ScriptPath = Join-Path $ScriptDir "k6\$Scenario.js"

if (-not (Test-Path $K6ScriptPath)) {
    Write-Error "Script not found: $K6ScriptPath"
    exit 1
}

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " Running k6 Scenario: $Scenario" -ForegroundColor Cyan
Write-Host " Target URL: $BaseUrl" -ForegroundColor Cyan
Write-Host " Tenant: $TenantSlug" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$k6Cmd = Get-Command k6 -ErrorAction SilentlyContinue

if ($k6Cmd) {
    Write-Host "[INFO] Using native k6 CLI" -ForegroundColor Green
    $env:BASE_URL = $BaseUrl
    $env:TENANT_SLUG = $TenantSlug
    & k6 run $K6ScriptPath
}
else {
    Write-Host "[INFO] Native k6 not detected. Running via grafana/k6 container..." -ForegroundColor Yellow

    # When running k6 from Docker on Windows, use host.docker.internal to hit localhost services
    $containerBaseUrl = $BaseUrl -replace "localhost", "host.docker.internal" -replace "127.0.0.1", "host.docker.internal"
    
    Get-Content $K6ScriptPath -Raw | docker run --rm -i -e "BASE_URL=$containerBaseUrl" -e "TENANT_SLUG=$TenantSlug" grafana/k6:latest run -
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n[FAIL] k6 thresholds violated or execution failed." -ForegroundColor Red
    exit $LASTEXITCODE
}
else {
    Write-Host "`n[PASS] All k6 thresholds satisfied successfully." -ForegroundColor Green
}
