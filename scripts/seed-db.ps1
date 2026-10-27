#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Executes on-demand database seeding for RestoCore Backend.
.DESCRIPTION
    Idempotently seeds PostgreSQL with the base tenant (el-gato-negro),
    hashed user accounts, menu categories, sample dishes, and QR tables.
#>

$ErrorActionPreference = "Stop"
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " RestoCore Database Seeder (On-Demand)" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

Push-Location $RepoRoot
try {
    dotnet run --project src/RestoCore.Api/RestoCore.Api.csproj -- --seed
    if ($LASTEXITCODE -ne 0) {
        throw "Database seeding exited with code $LASTEXITCODE"
    }
    Write-Host "`n[OK] Database seeded successfully." -ForegroundColor Green
}
finally {
    Pop-Location
}
