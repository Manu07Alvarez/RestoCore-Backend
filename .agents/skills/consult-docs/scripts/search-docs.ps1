param(
    [Parameter(Mandatory = $true)]
    [string]$Query,

    [ValidateSet("all", "adr", "openapi", "ears", "prd", "graphify")]
    [string]$Section = "all",

    [string]$DocsPath = "R:\Programando\RestoCore-Docs"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $DocsPath)) {
    Write-Error "El directorio de documentacion '$DocsPath' no existe."
    exit 1
}

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " Búsqueda en RestoCore-Docs: '$Query' [Sección: $Section]" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Definir rutas según sección
$targetFiles = @()

switch ($Section) {
    "adr" {
        $targetFiles = Get-ChildItem -Path (Join-Path $DocsPath "docs/adr") -Filter "*.md" -Recurse
    }
    "openapi" {
        $targetFiles = Get-ChildItem -Path (Join-Path $DocsPath "specs") -Filter "*openapi*.yaml" -Recurse
    }
    "ears" {
        $targetFiles = Get-ChildItem -Path (Join-Path $DocsPath "docs") -Filter "*ears*.md" -Recurse
    }
    "prd" {
        $targetFiles = Get-ChildItem -Path (Join-Path $DocsPath "docs") -Filter "*prd*.md" -Recurse
    }
    "graphify" {
        $targetFiles = Get-ChildItem -Path (Join-Path $DocsPath "graphify-out") -Filter "*.md" -Recurse
    }
    default {
        $docsMd = Get-ChildItem -Path (Join-Path $DocsPath "docs") -Filter "*.md" -Recurse | Where-Object { $_.FullName -notmatch "node_modules|\.git" }
        $specsYaml = Get-ChildItem -Path (Join-Path $DocsPath "specs") -Filter "*.yaml" -Recurse | Where-Object { $_.FullName -notmatch "node_modules|\.git" }
        $graphifyMd = Get-ChildItem -Path (Join-Path $DocsPath "graphify-out") -Filter "*.md" -Recurse -ErrorAction SilentlyContinue
        $targetFiles = @($docsMd) + @($specsYaml) + @($graphifyMd)
    }
}

$resultsFound = 0

foreach ($file in $targetFiles) {
    if (-not (Test-Path $file.FullName)) { continue }
    
    $matches = Select-String -Path $file.FullName -Pattern $Query -CaseSensitive:$false -SimpleMatch:$false
    if ($matches) {
        $resultsFound += $matches.Count
        $relPath = $file.FullName.Replace($DocsPath, "").TrimStart("\/")
        Write-Host "`n[$relPath] ($($matches.Count) coincidencias)" -ForegroundColor Yellow
        
        # Mostrar los primeros 3 resultados con contexto
        $matches | Select-Object -First 3 | ForEach-Object {
            $lineNum = $_.LineNumber
            $text = $_.Line.Trim()
            if ($text.Length -gt 130) { $text = $text.Substring(0, 130) + "..." }
            Write-Host "  Línea $lineNum : $text" -ForegroundColor Gray
        }
    }
}

Write-Host "`n----------------------------------------------------------" -ForegroundColor Cyan
Write-Host "Total de coincidencias encontradas: $resultsFound" -ForegroundColor Cyan
Write-Host "----------------------------------------------------------`n" -ForegroundColor Cyan
