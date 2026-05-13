param(
    [string]$OutputName = "ManuscriptCalculator.exe"
)

$ErrorActionPreference = "Stop"

$compilerCandidates = @(
    "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe",
    "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)

$csc = $compilerCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $csc) {
    throw "C# compiler not found. Please install .NET Framework 4.x."
}

$root = $PSScriptRoot
$srcDir = Join-Path $root "src\ManuscriptCalculator"
$outDir = Join-Path $root "dist"
$exePath = Join-Path $outDir $OutputName
$manifestPath = Join-Path $srcDir "app.manifest"
$appConfigPath = Join-Path $srcDir "app.config"

New-Item -ItemType Directory -Force $outDir | Out-Null

$sources = Get-ChildItem -Path $srcDir -Filter *.cs | Sort-Object Name | ForEach-Object { $_.FullName }
if (-not $sources) {
    throw "No source files were found."
}

& $csc `
    /nologo `
    /target:winexe `
    /platform:anycpu `
    /optimize+ `
    /codepage:65001 `
    /out:$exePath `
    /win32manifest:$manifestPath `
    /r:System.dll `
    /r:System.Core.dll `
    /r:System.Drawing.dll `
    /r:System.Windows.Forms.dll `
    /r:System.Numerics.dll `
    $sources

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

if (Test-Path $appConfigPath) {
    Copy-Item -Path $appConfigPath -Destination ($exePath + ".config") -Force
}

Write-Host ""
Write-Host "Build succeeded:" -ForegroundColor Green
Write-Host "  $exePath"
